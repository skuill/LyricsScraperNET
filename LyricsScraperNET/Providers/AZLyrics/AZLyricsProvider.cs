using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network;
using LyricsScraperNET.Providers.Abstract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.AZLyrics
{
    public sealed class AZLyricsProvider : ExternalProviderBase
    {
        private ILogger<AZLyricsProvider>? _logger;
        private readonly IExternalUriConverter _uriConverter;

        private const string _lyricStart = "<!-- Usage of azlyrics.com content by any third-party lyrics provider is prohibited by our licensing agreement. Sorry about that. -->";
        private const string _lyricEnd = "<!-- MxM banner -->";

        #region Constructors

        public AZLyricsProvider()
        {
            Parser = new AZLyricsParser();
            WebClient = new HtmlAgilityWebClient();
            Options = new AZLyricsOptions() { Enabled = true };
            _uriConverter = new AZLyricsUriConverter();
        }

        public AZLyricsProvider(ILogger<AZLyricsProvider> logger, AZLyricsOptions options) : this()
        {
            _logger = logger;
            Ensure.ArgumentNotNull(options, nameof(options));
            Options = options;
        }

        public AZLyricsProvider(ILogger<AZLyricsProvider> logger, IOptionsSnapshot<AZLyricsOptions> options)
            : this(logger, options.Value)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        public AZLyricsProvider(AZLyricsOptions options)
            : this(null, options)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        public AZLyricsProvider(IOptionsSnapshot<AZLyricsOptions> options)
            : this(null, options.Value)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        #endregion

        public override IExternalProviderOptions Options { get; }

        #region Sync

        protected override SearchResult SearchLyric(string artist, string song, CancellationToken cancellationToken)
        {
            return SearchLyric(_uriConverter.GetLyricUri(artist, song), cancellationToken);
        }

        protected override SearchResult SearchLyric(Uri uri, CancellationToken cancellationToken)
        {
            if (WebClient == null || Parser == null)
            {
                _logger?.LogWarning($"AZLyrics. Please set up WebClient and Parser first");
                return new SearchResult(Models.ExternalProviderType.AZLyrics);
            }
            var text = WebClient.Load(uri, cancellationToken);
            return PostProcessLyric(uri, text);
        }

        #endregion

        #region Async

        protected override async Task<SearchResult> SearchLyricAsync(string artist, string song, CancellationToken cancellationToken)
        {
            return await SearchLyricAsync(_uriConverter.GetLyricUri(artist, song), cancellationToken);
        }

        protected override async Task<SearchResult> SearchLyricAsync(Uri uri, CancellationToken cancellationToken)
        {
            if (WebClient == null || Parser == null)
            {
                _logger?.LogWarning($"AZLyrics. Please set up WebClient and Parser first");
                return new SearchResult(Models.ExternalProviderType.AZLyrics);
            }
            var text = await WebClient.LoadAsync(uri, cancellationToken);
            return PostProcessLyric(uri, text);
        }

        #endregion

        public override void WithLogger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AZLyricsProvider>();
        }

        private SearchResult PostProcessLyric(Uri uri, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                _logger?.LogWarning($"AZLyrics. Text is empty for Uri: [{uri}]");
                return new SearchResult(Models.ExternalProviderType.AZLyrics);
            }

            var startIndex = text.IndexOf(_lyricStart);
            var endIndex = text.IndexOf(_lyricEnd);
            if (startIndex <= 0 || endIndex <= 0)
            {
                _logger?.LogWarning($"AZLyrics. Can't find lyrics for Uri: [{uri}]");
                return new SearchResult(Models.ExternalProviderType.AZLyrics);
            }

            string result = Parser.Parse(text.Substring(startIndex, endIndex - startIndex));

            return new SearchResult(result, Models.ExternalProviderType.AZLyrics);
        }
    }
}
