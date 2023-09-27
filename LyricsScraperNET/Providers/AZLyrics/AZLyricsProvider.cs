using LyricsScraperNET.Extensions;
using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network;
using LyricsScraperNET.Providers.Abstract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.AZLyrics
{
    public sealed class AZLyricsProvider : ExternalProviderBase
    {
        private readonly ILogger<AZLyricsProvider> _logger;

        private const string _baseUri = "http://www.azlyrics.com/lyrics/";

        private const string _lyricStart = "<!-- Usage of azlyrics.com content by any third-party lyrics provider is prohibited by our licensing agreement. Sorry about that. -->";
        private const string _lyricEnd = "<!-- MxM banner -->";

        public Uri BaseUri => new Uri(_baseUri);

        public AZLyricsProvider()
        {
            Parser = new AZLyricsParser();
            WebClient = new HtmlAgilityWebClient();
            Options = new AZLyricsOptions() { Enabled = true };
        }

        public AZLyricsProvider(ILogger<AZLyricsProvider> logger, AZLyricsOptions aZLyricsOptions) : this()
        {
            _logger = logger;
            Ensure.ArgumentNotNull(aZLyricsOptions, nameof(aZLyricsOptions));
            Options = aZLyricsOptions;
        }

        public AZLyricsProvider(ILogger<AZLyricsProvider> logger, IOptionsSnapshot<AZLyricsOptions> aZLyricsOptions)
            : this(logger, aZLyricsOptions.Value)
        {
            Ensure.ArgumentNotNull(aZLyricsOptions, nameof(aZLyricsOptions));
        }

        public override IExternalProviderOptions Options { get; }

        #region Sync

        protected override SearchResult SearchLyric(string artist, string song)
        {
            return SearchLyric(GetLyricUri(artist, song));
        }

        protected override SearchResult SearchLyric(Uri uri)
        {
            if (WebClient == null || Parser == null)
            {
                _logger?.LogWarning($"AZLyrics. Please set up WebClient and Parser first");
                return new SearchResult();
            }
            var text = WebClient.Load(uri);
            return PostProcessLyric(uri, text);
        }

        #endregion


        #region Async

        protected override async Task<SearchResult> SearchLyricAsync(string artist, string song)
        {
            return await SearchLyricAsync(GetLyricUri(artist, song));
        }

        protected override async Task<SearchResult> SearchLyricAsync(Uri uri)
        {
            if (WebClient == null || Parser == null)
            {
                _logger?.LogWarning($"AZLyrics. Please set up WebClient and Parser first");
                return new SearchResult();
            }
            var text = await WebClient.LoadAsync(uri);
            return PostProcessLyric(uri, text);
        }

        #endregion


        private Uri GetLyricUri(string artist, string song)
        {
            // http://www.azlyrics.com/lyrics/youngthug/richniggashit.htm
            // remove articles from artist on start. For example for band [The Devil Wears Prada]: https://www.azlyrics.com/d/devilwearsprada.html
            var artistStripped = artist.ToLowerInvariant().StripRedundantChars(true);
            var titleStripped = song.ToLowerInvariant().StripRedundantChars();
            return new Uri(BaseUri, $"{artistStripped}/{titleStripped}.html");
        }

        private SearchResult PostProcessLyric(Uri uri, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                _logger?.LogWarning($"AZLyrics. Text is empty for {uri}");
                return new SearchResult();
            }

            var startIndex = text.IndexOf(_lyricStart);
            var endIndex = text.IndexOf(_lyricEnd);
            if (startIndex <= 0 || endIndex <= 0)
            {
                _logger?.LogWarning($"AZLyrics. Can't find lyrics for {uri}");
                return new SearchResult();
            }

            string result = Parser.Parse(text.Substring(startIndex, endIndex - startIndex));

            return new SearchResult(result, Models.ExternalProviderType.AZLyrics);
        }
    }
}
