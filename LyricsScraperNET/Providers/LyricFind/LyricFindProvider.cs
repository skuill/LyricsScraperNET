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

namespace LyricsScraperNET.Providers.LyricFind
{
    public sealed class LyricFindProvider : ExternalProviderBase
    {
        private ILogger<LyricFindProvider> _logger;
        private readonly IExternalUriConverter _uriConverter;

        /// <summary>
        /// Field name in json format that contains lyric text.
        /// </summary>
        private const string _lyricStart = "\"lyrics\"";

        // "instrumental":true
        private const string _instrumentalStart = "\"instrumental\"";
        // "songIsInstrumental":true
        private const string _songIsInstrumentalStart = "\"songIsInstrumental\"";

        // "viewable":false
        private const string _viewableStart = "\"viewable\"";
        // "response":{"code":206,"description":"LYRIC NOT AVAILABLE: BLOCKED"}
        private const string _lyricNotAvailablePattern = @"\{[^}]*(""response""\s*:\s*\{[^}]*(""code""\s*:\s*206)[^}]*(""description""\s*:\s*""LYRIC NOT AVAILABLE: BLOCKED"")[^}]*\})[^}]*\}|\{[^}]*(""response""\s*:\s*\{[^}]*(""description""\s*:\s*""LYRIC NOT AVAILABLE: BLOCKED"")[^}]*(""code""\s*:\s*206)[^}]*\})[^}]*\}";

        #region Constructors

        public LyricFindProvider()
        {
            Parser = new LyricFindParser();
            WebClient = new HtmlAgilityWebClient();
            Options = new LyricFindOptions() { Enabled = true };
            _uriConverter = new LyricFindUriConverter();
        }

        public LyricFindProvider(ILogger<LyricFindProvider> logger, LyricFindOptions options)
            : this()
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

        protected override SearchResult SearchLyric(string artist, string song, CancellationToken cancellationToken = default)
        {
            return SearchLyric(_uriConverter.GetLyricUri(artist, song), cancellationToken);
        }

        protected override SearchResult SearchLyric(Uri uri, CancellationToken cancellationToken = default)
        {
            return SearchLyricAsync(uri, cancellationToken).GetAwaiter().GetResult();
        }

        #endregion

        #region Async

        protected override async Task<SearchResult> SearchLyricAsync(string artist, string song, CancellationToken cancellationToken = default)
        {
            return await SearchLyricAsync(_uriConverter.GetLyricUri(artist, song), cancellationToken);
        }

        protected override async Task<SearchResult> SearchLyricAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            if (WebClient == null || Parser == null)
            {
                _logger?.LogWarning($"LyricFind. Please set up WebClient and Parser first");
                return new SearchResult(Models.ExternalProviderType.LyricFind);
            }
            var text = await WebClient.LoadAsync(uri, cancellationToken);
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
                _logger?.LogWarning($"LyricFind. Text is empty for Uri: [{uri}]");
                return new SearchResult(Models.ExternalProviderType.LyricFind);
            }

            var startIndex = text.IndexOf(_lyricStart);
            if (startIndex <= 0)
            {
                // In case of instrumental song it could not contains lyric field.
                if (IsInstumentalLyric(text))
                    return new SearchResult(Models.ExternalProviderType.LyricFind).AddInstrumental(true);

                // In case if lyrics existed, but "These lyrics are not available in your region currently."
                if (IsRegionRestrictedLyric(text))
                    return new SearchResult(Models.ExternalProviderType.LyricFind, ResponseStatusCode.RegionRestricted);

                _logger?.LogWarning($"LyricFind. Can't find lyrics for Uri: [{uri}]");
                return new SearchResult(Models.ExternalProviderType.LyricFind);
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
                // In case of instrumental song it could not contains lyric field.
                if (IsInstumentalLyric(text))
                    return new SearchResult(Models.ExternalProviderType.LyricFind).AddInstrumental(true);

                _logger?.LogWarning($"LyricFind. Can't parse lyrics for Uri: [{uri}]");
                return new SearchResult(Models.ExternalProviderType.LyricFind);
            }

            string result = Parser.Parse(text.Substring(start, endOfLyricInJson - start));

            return new SearchResult(result, Models.ExternalProviderType.LyricFind);
        }


        /// <summary>
        /// Check if lyric text contains region restricted information like viewable (false) and repsonse with code (206) and description.
        /// </summary>
        private bool IsRegionRestrictedLyric(string text)
        {
            return TryReturnBooleanFieldValue(text, _viewableStart, "false")
                && Regex.IsMatch(text, _lyricNotAvailablePattern);
        }

        /// <summary>
        /// Check if lyric text contains instrumental flag.
        /// </summary>
        private bool IsInstumentalLyric(string text)
        {
            return TryReturnBooleanFieldValue(text, _instrumentalStart)
                || TryReturnBooleanFieldValue(text, _songIsInstrumentalStart);
        }

        /// <summary>
        /// Try to find and return the fielad value as boolean. Pattern: [<paramref name="fieldName"/>:true(or false)].
        /// In case if fieldName is not found returns false.
        /// </summary>
        private bool TryReturnBooleanFieldValue(string text, string fieldName, string booleanValue = "true")
        {
            var startIndex = text.IndexOf(fieldName);
            if (startIndex <= 0)
                return false;
            var fieldValue = text.Substring(startIndex + fieldName.Length + 1, 5);
            return fieldValue.IndexOf(booleanValue, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
