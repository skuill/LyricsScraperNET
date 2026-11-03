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
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.LyricsFreak
{
    public sealed class LyricsFreakProvider : ExternalProviderBase
    {
        private ILogger<LyricsFreakProvider>? _logger;
        private readonly LyricsFreakUriConverter _uriConverter;

        private const string SongLinksXPath = "//a[@href and @title]";
        private const string SongLinkTitleSuffix = " Lyrics";
        private const string LyricsDivXPath = "//div[@data-container-id='lyrics']";

        private const string PageNotFoundText = "#404 - Page Not Found";

        public override IExternalProviderOptions Options { get; }

        #region Constructors

        public LyricsFreakProvider()
        {
            Parser = new LyricsFreakParser();
            WebClient = new NetHttpClient();
            Options = new LyricsFreakOptions() { Enabled = true };
            _uriConverter = new LyricsFreakUriConverter();
        }

        public LyricsFreakProvider(ILogger<LyricsFreakProvider> logger, LyricsFreakOptions options)
            : this()
        {
            _logger = logger;
            Ensure.ArgumentNotNull(options, nameof(options));
            Options = options;
        }

        public LyricsFreakProvider(ILogger<LyricsFreakProvider> logger, IOptionsSnapshot<LyricsFreakOptions> options)
            : this(logger, options.Value)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        public LyricsFreakProvider(LyricsFreakOptions options)
            : this(NullLogger<LyricsFreakProvider>.Instance, options)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        public LyricsFreakProvider(IOptionsSnapshot<LyricsFreakOptions> options)
            : this(NullLogger<LyricsFreakProvider>.Instance, options.Value)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        #endregion

        #region Async

        protected override async Task<SearchResult> SearchLyricAsync(string artist, string song, CancellationToken cancellationToken = default)
        {
            if (WebClient == null || Parser == null)
            {
                _logger?.LogWarning($"LyricsFreak. Please set up WebClient and Parser first");
                return new SearchResult(ExternalProviderType.LyricsFreak);
            }

            // 1. Open the artist's page.
            var htmlResponse = string.Empty;
            foreach (var artistUri in _uriConverter.GetArtistUris(artist))
            {
                htmlResponse = await LoadPageAsync(artistUri, cancellationToken).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                // A missing page is a 404 (empty body) or a page with the 404 text: try the next candidate.
                if (!string.IsNullOrEmpty(htmlResponse) && !htmlResponse.Contains(PageNotFoundText))
                    break;
            }

            if (string.IsNullOrEmpty(htmlResponse) || htmlResponse.Contains(PageNotFoundText))
            {
                _logger?.LogWarning($"LyricsFreak. Artist's page not found (404). [{artist}]. Song name: [{song}]");
                return new SearchResult(ExternalProviderType.LyricsFreak);
            }

            // 2. Find song on the artist page and get link to the web page.
            var songHref = GetSongHrefFromHtmlBody(htmlResponse, song);
            if (string.IsNullOrEmpty(songHref))
            {
                _logger?.LogWarning($"LyricsFreak. Can't find song Uri for artist: [{artist}]. Song name: [{song}]");
                return new SearchResult(ExternalProviderType.LyricsFreak);
            }
            var songUri = new Uri(LyricsFreakUriConverter.BaseUrl + songHref);

            return await SearchLyricAsync(songUri, cancellationToken).ConfigureAwait(false);
        }

        protected async override Task<SearchResult> SearchLyricAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            var htmlBodyContent = await LoadPageAsync(uri, cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var songLyrics = GetSongLyricsFromHtmlBody(htmlBodyContent);
            if (string.IsNullOrEmpty(songLyrics))
            {
                _logger?.LogWarning($"LyricsFreak. Can't find lyrics for song's uri: [{uri}]");
                return new SearchResult(ExternalProviderType.LyricsFreak);
            }

            var lyricsText = Parser.Parse(songLyrics);

            return new SearchResult(lyricsText, ExternalProviderType.LyricsFreak);
        }

        #endregion

        public override void WithLogger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LyricsFreakProvider>();
        }

        #region Private methods

        /// <summary>
        /// Song links on the artist page have the title "{song} Lyrics", for example "Sweet Child O&amp;#039; Mine Lyrics".
        /// The song name is compared with <see cref="TextMatchingExtensions.IsSameText"/>, so "Sweet Child O Mine" and "Avohai" find "Avôhai".
        /// </summary>
        private static string GetSongHrefFromHtmlBody(string htmlBody, string song)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlBody);

            var linkNodes = htmlDocument.DocumentNode.SelectNodes(SongLinksXPath);
            if (linkNodes == null)
                return string.Empty;

            foreach (var linkNode in linkNodes)
            {
                var title = HtmlEntity.DeEntitize(linkNode.GetAttributeValue("title", string.Empty)).Trim();
                if (!title.EndsWith(SongLinkTitleSuffix, StringComparison.OrdinalIgnoreCase))
                    continue;

                var songTitle = title.Substring(0, title.Length - SongLinkTitleSuffix.Length);
                if (songTitle.IsSameText(song))
                    return linkNode.GetAttributeValue("href", string.Empty);
            }

            return string.Empty;
        }

        private static string GetSongLyricsFromHtmlBody(string htmlBody)
        {
            var lyricsNode = htmlBody.SelectSingleNodeByXPath(LyricsDivXPath);

            if (lyricsNode == null)
                return string.Empty;

            string lyricsText = lyricsNode.InnerText.Trim();
            return lyricsText;
        }

        #endregion
    }
}
