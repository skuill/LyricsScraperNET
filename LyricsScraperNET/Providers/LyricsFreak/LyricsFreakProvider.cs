using LyricsScraperNET.Extensions;
using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network;
using LyricsScraperNET.Providers.Abstract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.LyricsFreak
{
    internal class LyricsFreakProvider : ExternalProviderBase
    {
        private ILogger<LyricsFreakProvider>? _logger;
        private readonly IExternalUriConverter _uriConverter;
        private readonly string LyricsHrefXPath = "//a[translate(@title, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '{0} lyrics']";
        private const string LyricsDivXPath = "//div[@data-container-id='lyrics']";

        public override IExternalProviderOptions Options { get; }

        #region Constructors

        public LyricsFreakProvider()
        {
            Parser = new LyricsFreakParser();
            WebClient = new HtmlAgilityWebClient();
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

        #region Sync

        protected override SearchResult SearchLyric(string artist, string song, CancellationToken cancellationToken = default)
        {
            return SearchLyricAsync(artist, song, cancellationToken).GetAwaiter().GetResult();
        }

        protected override SearchResult SearchLyric(Uri uri, CancellationToken cancellationToken = default)
        {
            return SearchLyricAsync(uri, cancellationToken).GetAwaiter().GetResult();
        }

        #endregion

        #region Async

        protected override async Task<SearchResult> SearchLyricAsync(string artist, string song, CancellationToken cancellationToken = default)
        {
            if (WebClient == null || Parser == null)
            {
                _logger?.LogWarning($"LyricsFreak. Please set up WebClient and Parser first");
                return new SearchResult(Models.ExternalProviderType.LyricsFreak);
            }

            // 1. Open the artist's page.
            var artistUri = _uriConverter.GetArtistUri(artist);

            var htmlResponse = await WebClient.LoadAsync(artistUri, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            // 2. Find song on the artist page and get link to the web page.
            var songHref = GetSongHrefFromHtmlBody(htmlResponse, song);
            if (string.IsNullOrEmpty(songHref))
            {
                _logger?.LogWarning($"LyricsFreak. Can't find song Uri for song: [{song}]");
                return new SearchResult(Models.ExternalProviderType.LyricsFreak);
            }
            var songUri = new Uri(LyricsFreakUriConverter.BaseUrl + songHref);

            return await SearchLyricAsync(songUri, cancellationToken);
        }

        protected async override Task<SearchResult> SearchLyricAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            var text = await WebClient.LoadAsync(uri, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var songLyrics = GetSongLyricsFromHtmlBody(text);
            if (string.IsNullOrEmpty(songLyrics))
            {
                _logger?.LogWarning($"LyricsFreak. Can't find song lyrics for song uri: [{uri.AbsoluteUri}]");
                return new SearchResult(Models.ExternalProviderType.LyricsFreak);
            }

            var lyricsText = Parser.Parse(songLyrics);

            return new SearchResult(lyricsText, Models.ExternalProviderType.LyricsFreak);
        }

        #endregion

        #region Private methods

        private string GetSongHrefFromHtmlBody(string htmlBody, string song)
        {
            string formattedXPath = string.Format(LyricsHrefXPath, GetEncodedSong(song));
            var linkNode = htmlBody.SelectSingleNodeByXPath(formattedXPath);
            if (linkNode == null)
            {
                return string.Empty;
            }

            string hrefSong = linkNode.GetAttributeValue("href", string.Empty);
            return hrefSong;
        }

        private string GetSongLyricsFromHtmlBody(string htmlBody)
        {
            var lyricsNode = htmlBody.SelectSingleNodeByXPath(LyricsDivXPath);
            if (lyricsNode == null)
            {
                return string.Empty;

            }
            string lyricsText = lyricsNode.InnerText.Trim();
            return lyricsText;
        }

        private string GetEncodedSong(string song)
        {
            string encodedSong = System.Net.WebUtility.HtmlEncode(song).ToLowerInvariant();
            encodedSong = encodedSong.Replace("&#39;", "&#039;");
            return encodedSong;
        }

        #endregion
    }
}
