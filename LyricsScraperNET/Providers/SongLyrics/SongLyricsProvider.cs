using HtmlAgilityPack;
using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network;
using LyricsScraperNET.Providers.Abstract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.SongLyrics
{
    public sealed class SongLyricsProvider : ExternalProviderBase
    {
        private readonly ILogger<SongLyricsProvider> _logger;
        private readonly IExternalUriConverter _uriConverter;


        private const string _lyricsContainerNodesXPath = "//p[contains(@id, 'songLyricsDiv')]";

        private const string NotExistLyricPattern = "We do not have the lyrics for (.*) yet.";

        #region Constructors

        public SongLyricsProvider()
        {
            WebClient = new NetHttpClient();
            Options = new SongLyricsOptions() { Enabled = true };
            _uriConverter = new SongLyricsUriConverter();
        }

        public SongLyricsProvider(ILogger<SongLyricsProvider> logger, SongLyricsOptions options) : this()
        {
            _logger = logger;
            Ensure.ArgumentNotNull(options, nameof(options));
            Options = options;
        }

        public SongLyricsProvider(ILogger<SongLyricsProvider> logger, IOptionsSnapshot<SongLyricsOptions> options)
            : this(logger, options.Value)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        public SongLyricsProvider(SongLyricsOptions options)
            : this(null, options)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        public SongLyricsProvider(IOptionsSnapshot<SongLyricsOptions> options)
            : this(null, options.Value)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        #endregion

        public override IExternalProviderOptions Options { get; }


        #region Sync

        protected override SearchResult SearchLyric(string artist, string song)
        {
            return SearchLyric(_uriConverter.GetLyricUri(artist, song));
        }

        protected override SearchResult SearchLyric(Uri uri)
        {
            if (WebClient == null)
            {
                _logger?.LogWarning($"SongLyrics. Please set up WebClient first");
                return new SearchResult();
            }
            var htmlPageBody = WebClient.Load(uri);
            return GetParsedLyricFromHtmlPageBody(uri, htmlPageBody);
        }

        #endregion


        #region Async

        protected override async Task<SearchResult> SearchLyricAsync(string artist, string song)
        {
            return await SearchLyricAsync(_uriConverter.GetLyricUri(artist, song));
        }

        protected override async Task<SearchResult> SearchLyricAsync(Uri uri)
        {
            if (WebClient == null)
            {
                _logger?.LogWarning($"SongLyrics. Please set up WebClient first");
                return new SearchResult();
            }
            var htmlPageBody = await WebClient.LoadAsync(uri);
            return GetParsedLyricFromHtmlPageBody(uri, htmlPageBody);
        }

        #endregion


        private SearchResult GetParsedLyricFromHtmlPageBody(Uri uri, string htmlPageBody)
        {
            if (string.IsNullOrEmpty(htmlPageBody))
            {
                _logger?.LogWarning($"SongLyrics. Text is empty for {uri}");
                return new SearchResult();
            }

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlPageBody);

            var lyricsContainerNode = htmlDocument.DocumentNode.SelectSingleNode(_lyricsContainerNodesXPath);

            if (lyricsContainerNode == null)
            {
                _logger?.LogWarning($"SongLyrics. Can't find lyrics for {uri}");
                return new SearchResult();
            }

            if (Regex.IsMatch(lyricsContainerNode.InnerText, NotExistLyricPattern, RegexOptions.IgnoreCase))
            {
                _logger?.LogDebug($"SongLyrics. Returns empty result: \"{lyricsContainerNode.InnerText}\"");
                return new SearchResult();
            }

            return new SearchResult(lyricsContainerNode.InnerText, Models.ExternalProviderType.SongLyrics);
        }
    }
}
