using HtmlAgilityPack;
using LyricsScraperNET.Extensions;
using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.KPopLyrics
{
    public sealed class KPopLyricsProvider : ExternalProviderBase
    {
        private ILogger<KPopLyricsProvider>? _logger;
        private readonly IExternalUriConverter _uriConverter;

        private const string LyricsContainerNodesXPath = "//*[contains(@class, 'entry-content') and contains(@class, 'mh-clearfix')]";

        // Post titles look like "BTS – Dynamite Lyrics".
        private const string PostTitleSeparator = " – ";
        private const string PostTitleSuffix = " Lyrics";

        #region Constructors

        public KPopLyricsProvider()
        {
            Parser = new KPopLyricsParser();
            WebClient = new NetHttpClient();
            Options = new KPopLyricsOptions() { Enabled = true };
            _uriConverter = new KPopLyricsUriConverter();
        }

        public KPopLyricsProvider(ILogger<KPopLyricsProvider> logger, KPopLyricsOptions options)
            : this()
        {
            _logger = logger;
            Ensure.ArgumentNotNull(options, nameof(options));
            Options = options;
        }

        public KPopLyricsProvider(ILogger<KPopLyricsProvider> logger, IOptionsSnapshot<KPopLyricsOptions> options)
            : this(logger, options.Value)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        public KPopLyricsProvider(KPopLyricsOptions options)
            : this(NullLogger<KPopLyricsProvider>.Instance, options)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        public KPopLyricsProvider(IOptionsSnapshot<KPopLyricsOptions> options)
            : this(NullLogger<KPopLyricsProvider>.Instance, options.Value)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        #endregion

        public override IExternalProviderOptions Options { get; }

        #region Async

        protected override async Task<SearchResult> SearchLyricAsync(string artist, string song, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested(); // Ensure cancellation is handled early

            if (WebClient == null || Parser == null)
            {
                _logger?.LogWarning($"KPopLyrics. Please set up WebClient and Parser first");
                return new SearchResult(ExternalProviderType.KPopLyrics);
            }

            var titleArtist = KPopLyricsUriConverter.GetTitleName(artist);
            var titleSong = KPopLyricsUriConverter.GetTitleName(song);
            if (!KPopLyricsUriConverter.HasLatinLetterOrDigit(titleArtist) || !KPopLyricsUriConverter.HasLatinLetterOrDigit(titleSong))
            {
                _logger?.LogInformation($"KPopLyrics. Post titles are in Latin letters only, nothing to search for artist: [{artist}]. Song name: [{song}]");
                return new SearchResult(ExternalProviderType.KPopLyrics);
            }

            // 1. Search the posts. Page URLs can't be built from the names: the site redirects a guessed URL
            // to the closest post, which can be another song ("bts-dynamite" opened the Holiday Remix).
            var searchUri = _uriConverter.GetLyricUri(artist, song);
            var searchResponse = await LoadPageAsync(searchUri, cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            // 2. Take the post with the same artist and song.
            var songUri = GetSongUri(searchUri, searchResponse, titleArtist, titleSong);
            if (songUri == null)
            {
                _logger?.LogWarning($"KPopLyrics. Can't find song Uri for artist: [{artist}]. Song name: [{song}]");
                return new SearchResult(ExternalProviderType.KPopLyrics);
            }

            return await SearchLyricAsync(songUri, cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<SearchResult> SearchLyricAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            if (WebClient == null || Parser == null)
            {
                _logger?.LogWarning($"KPopLyrics. Please set up WebClient and Parser first");
                return new SearchResult(ExternalProviderType.KPopLyrics);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var text = await LoadPageAsync(uri, cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            return PostProcessLyric(uri, text);
        }

        #endregion

        public override void WithLogger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<KPopLyricsProvider>();
        }

        /// <summary>
        /// The link of the post titled "{artist} – {song} Lyrics", compared like Genius results:
        /// "Dynamite" does not match "Dynamite (Holiday Remix)".
        /// </summary>
        private Uri? GetSongUri(Uri searchUri, string searchResponse, string artist, string song)
        {
            // An empty body is a failed load, already recorded by LoadPageAsync.
            if (string.IsNullOrWhiteSpace(searchResponse))
                return null;

            List<KPopLyricsPost>? posts;
            try
            {
                posts = JsonSerializer.Deserialize<List<KPopLyricsPost>>(searchResponse);
            }
            catch (JsonException ex)
            {
                _logger?.LogWarning($"KPopLyrics. Failed to parse the search response for Uri: [{searchUri}]. Exception: {ex}");
                return null;
            }

            foreach (var post in posts ?? [])
            {
                if (!TryParsePostTitle(post.Title?.Rendered, out var postArtist, out var postSong)
                    || !postArtist.IsSameText(artist)
                    || !postSong.IsSameText(song))
                    continue;

                // Load only pages of the site.
                if (Uri.TryCreate(post.Link, UriKind.Absolute, out var songUri)
                    && songUri.Scheme == Uri.UriSchemeHttps
                    && songUri.Host == KPopLyricsUriConverter.Host)
                    return songUri;
            }

            return null;
        }

        private static bool TryParsePostTitle(string? renderedTitle, out string artist, out string song)
        {
            artist = song = string.Empty;
            if (string.IsNullOrEmpty(renderedTitle))
                return false;

            var title = HtmlEntity.DeEntitize(renderedTitle).Trim();
            if (!title.EndsWith(PostTitleSuffix, StringComparison.Ordinal))
                return false;

            var separatorIndex = title.IndexOf(PostTitleSeparator, StringComparison.Ordinal);
            if (separatorIndex <= 0)
                return false;

            artist = title[..separatorIndex];
            song = title[(separatorIndex + PostTitleSeparator.Length)..^PostTitleSuffix.Length];
            return true;
        }

        private SearchResult PostProcessLyric(Uri uri, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                _logger?.LogWarning($"KPopLyrics. Text is empty for Uri: [{uri}]");
                return new SearchResult(ExternalProviderType.KPopLyrics);
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(text);

            var mainNode = htmlDoc.DocumentNode?.SelectNodes(LyricsContainerNodesXPath)?.FirstOrDefault();

            if (mainNode is null)
            {
                _logger?.LogWarning($"KPopLyrics. Can't parse lyric from the page. Couldn't find lyrics container. Uri: {uri}");
                return new SearchResult(ExternalProviderType.KPopLyrics);
            }

            if (mainNode.OuterHtml.Contains("no-content-found"))
            {
                _logger?.LogInformation($"KPopLyrics. Page not found (404). Uri: {uri}");
                return new SearchResult(ExternalProviderType.KPopLyrics);
            }

            var h2Nodes = htmlDoc.DocumentNode?.SelectNodes("//h2");

            if (h2Nodes is null || h2Nodes.Count == 0)
            {
                _logger?.LogWarning($"KPopLyrics. Can't parse lyric from the page. Couldn't find header nodes. Uri: {uri}");
                return new SearchResult(ExternalProviderType.KPopLyrics);
            }

            // Sometimes lyrics have eng translation but sometimes its only romanized version.
            var h2Node = h2Nodes
                .FirstOrDefault(x => x.OuterHtml.Contains("Official English Translation")
                                  || x.OuterHtml.Contains("English Translation Lyrics"))
                ?? h2Nodes
                    .FirstOrDefault(x => x.OuterHtml.Contains("Romanized"));

            if (h2Node is null)
            {
                _logger?.LogWarning($"KPopLyrics. Can't parse lyric from the page. Couldn't find a valid lyrics header node. Uri: {uri}");
                return new SearchResult(ExternalProviderType.KPopLyrics);
            }

            var rawHtmlLyrics = TakeParagraphsUntilHeader(h2Node);

            if (string.IsNullOrEmpty(rawHtmlLyrics))
            {
                _logger?.LogWarning($"KPopLyrics. Can't parse lyric from the page. Couldn't extract lyrics content from paragraphs. Uri: {uri}");
                return new SearchResult(ExternalProviderType.KPopLyrics);
            }

            var result = Parser.Parse(rawHtmlLyrics);

            return new SearchResult(result, ExternalProviderType.KPopLyrics);
        }

        private static string TakeParagraphsUntilHeader(HtmlNode startNode)
        {
            var paragraphs = new List<string>();

            var sibling = startNode.NextSibling;
            while (sibling != null)
            {
                if (sibling.Name == "h2")
                    break;

                if (sibling.Name == "p")
                    paragraphs.Add(sibling.OuterHtml);
                else
                    break;

                sibling = sibling.NextSibling;
            }

            return string.Join("\n", paragraphs);
        }
    }
}