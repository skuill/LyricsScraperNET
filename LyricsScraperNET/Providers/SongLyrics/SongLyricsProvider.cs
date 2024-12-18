using HtmlAgilityPack;
using LyricsScraperNET.Extensions;
using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network;
using LyricsScraperNET.Providers.Abstract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.SongLyrics
{
    public sealed class SongLyricsProvider : ExternalProviderBase
    {
        private ILogger<SongLyricsProvider> _logger;
        private readonly IExternalUriConverter _uriConverter;

        /// <summary>
        /// The html node that contains part of the lyric.
        /// </summary>
        private const string LyricsContainerNodesXPath = "//p[contains(@id, 'songLyricsDiv')]";

        /// <summary>
        /// The text in the <see cref="LyricsContainerNodesXPath"/> if the lyrics do not exist on the site.
        /// </summary>
        private const string NotExistLyricPattern = "We do not have the lyrics for (.*) yet.";

        /// <summary>
        /// For instrumental songs, the text "[Instrumental]" is returned in the songLyricsDiv
        /// </summary>
        private const string InstrumentalLyricText = "Instrumental";

        #region Constructors

        public SongLyricsProvider()
        {
            WebClient = new NetHttpClient();
            Parser = new SongLyricsParser();
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

        protected override SearchResult SearchLyric(string artist, string song, CancellationToken cancellationToken = default)
        {
            return SearchLyric(_uriConverter.GetLyricUri(artist, song), cancellationToken);
        }

        protected override SearchResult SearchLyric(Uri uri, CancellationToken cancellationToken = default)
        {
            if (WebClient == null)
            {
                _logger?.LogWarning($"SongLyrics. Please set up WebClient first");
                return new SearchResult(Models.ExternalProviderType.SongLyrics);
            }
            var htmlPageBody = WebClient.Load(uri, cancellationToken);
            return GetParsedLyricFromHtmlPageBody(uri, htmlPageBody);
        }

        #endregion

        #region Async

        protected override async Task<SearchResult> SearchLyricAsync(string artist, string song, CancellationToken cancellationToken = default)
        {
            return await SearchLyricAsync(_uriConverter.GetLyricUri(artist, song), cancellationToken);
        }

        protected override async Task<SearchResult> SearchLyricAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            if (WebClient == null)
            {
                _logger?.LogWarning($"SongLyrics. Please set up WebClient first");
                return new SearchResult(Models.ExternalProviderType.SongLyrics);
            }
            var htmlPageBody = await WebClient.LoadAsync(uri, cancellationToken);
            return GetParsedLyricFromHtmlPageBody(uri, htmlPageBody);
        }

        #endregion

        public override void WithLogger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SongLyricsProvider>();
        }

        private SearchResult GetParsedLyricFromHtmlPageBody(Uri uri, string htmlPageBody)
        {
            if (string.IsNullOrEmpty(htmlPageBody))
            {
                _logger?.LogWarning($"SongLyrics. Text is empty for Uri: [{uri}]");
                return new SearchResult(Models.ExternalProviderType.SongLyrics);
            }

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlPageBody);

            var lyricsContainerNode = htmlDocument.DocumentNode.SelectSingleNode(LyricsContainerNodesXPath);

            if (lyricsContainerNode == null)
            {
                _logger?.LogWarning($"SongLyrics. Can't find lyrics for Uri: [{uri}]");
                return new SearchResult(Models.ExternalProviderType.SongLyrics);
            }

            // Check if lyric not exist on site yet
            if (Regex.IsMatch(lyricsContainerNode.InnerText, NotExistLyricPattern, RegexOptions.IgnoreCase))
            {
                _logger?.LogDebug($"SongLyrics. Returns empty result: \"{lyricsContainerNode.InnerText}\"");
                return new SearchResult(Models.ExternalProviderType.SongLyrics);
            }

            // Check if lyric is instrumental
            if (string.Equals(lyricsContainerNode.InnerText, InstrumentalLyricText, StringComparison.OrdinalIgnoreCase)
                || string.Equals(lyricsContainerNode.InnerText, $"[{InstrumentalLyricText}]", StringComparison.OrdinalIgnoreCase))
                return new SearchResult(Models.ExternalProviderType.SongLyrics).AddInstrumental(true);

            var parsedLyric = Parser.Parse(lyricsContainerNode.InnerText);

            return new SearchResult(parsedLyric, Models.ExternalProviderType.SongLyrics);
        }
    }
}
