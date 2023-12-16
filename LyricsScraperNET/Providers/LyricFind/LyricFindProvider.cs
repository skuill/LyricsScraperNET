using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network;
using LyricsScraperNET.Providers.Abstract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.LyricFind
{
    public sealed class LyricFindProvider : ExternalProviderBase
    {
        private ILogger<LyricFindProvider> _logger;
        private readonly IExternalUriConverter _uriConverter;

        private const string _lyricStart = "\"lyrics\"";

        #region Constructors

        public LyricFindProvider()
        {
            Parser = new LyricFindParser();
            WebClient = new HtmlAgilityWebClient();
            Options = new LyricFindOptions() { Enabled = true };
            _uriConverter = new LyricFindUriConverter();
        }

        public LyricFindProvider(ILogger<LyricFindProvider> logger, LyricFindOptions options) : this()
        {
            _logger = logger;
            Ensure.ArgumentNotNull(options, nameof(options));
            Options = options;
        }

        public LyricFindProvider(ILogger<LyricFindProvider> logger, IOptionsSnapshot<LyricFindOptions> options)
            : this(logger, options.Value)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        public LyricFindProvider(LyricFindOptions options)
            : this(null, options)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        public LyricFindProvider(IOptionsSnapshot<LyricFindOptions> options)
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
            if (WebClient == null || Parser == null)
            {
                _logger?.LogWarning($"LyricFind. Please set up WebClient and Parser first");
                return new SearchResult();
            }
            var text = WebClient.Load(uri);
            return PostProcessLyric(uri, text);
        }

        #endregion

        #region Async

        protected override async Task<SearchResult> SearchLyricAsync(string artist, string song)
        {
            return await SearchLyricAsync(_uriConverter.GetLyricUri(artist, song));
        }

        protected override async Task<SearchResult> SearchLyricAsync(Uri uri)
        {
            if (WebClient == null || Parser == null)
            {
                _logger?.LogWarning($"LyricFind. Please set up WebClient and Parser first");
                return new SearchResult();
            }
            var text = await WebClient.LoadAsync(uri);
            return PostProcessLyric(uri, text);
        }

        #endregion

        public override void WithLogger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LyricFindProvider>();
        }

        private SearchResult PostProcessLyric(Uri uri, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                _logger?.LogWarning($"LyricFind. Text is empty for {uri}");
                return new SearchResult();
            }

            var startIndex = text.IndexOf(_lyricStart);
            if (startIndex <= 0)
            {
                _logger?.LogWarning($"LyricFind. Can't find lyrics for {uri}");
                return new SearchResult();
            }

            // Trim the beginning of the text to the lyrics
            text = text.Substring(startIndex + _lyricStart.Length + 1);

            // Finding the end of the lyric text in the json field value.
            int start = text.IndexOf("\"") + 1;

            int endOfFieldValue = text.IndexOf("\",\"", start);
            int endOfJsonObject = text.IndexOf("\"}", start);
            int endOfLyricInJson = Math.Max(Math.Min(endOfFieldValue, endOfJsonObject), -1);
            if (endOfLyricInJson < 0)
            {
                _logger?.LogWarning($"LyricFind. Can't parse lyrics for {uri}");
                return new SearchResult();
            }

            string result = Parser.Parse(text.Substring(start, endOfLyricInJson - start));

            return new SearchResult(result, Models.ExternalProviderType.LyricFind);
        }
    }
}
