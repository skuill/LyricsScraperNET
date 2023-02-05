using LyricsScraperNET.Extensions;
using LyricsScraperNET.External.Abstract;
using LyricsScraperNET.Helpers;
using LyricsScraperNET.Network.Html;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LyricsScraperNET.External.AZLyrics
{
    public sealed class AZLyricsClient : ExternalServiceClientBase
    {
        private readonly ILogger<AZLyricsClient> _logger;

        private const string _baseUri = "http://www.azlyrics.com/lyrics/";

        private const string _lyricStart = "<!-- Usage of azlyrics.com content by any third-party lyrics provider is prohibited by our licensing agreement. Sorry about that. -->";
        private const string _lyricEnd = "<!-- MxM banner -->";

        public Uri BaseUri => new Uri(_baseUri);

        public AZLyricsClient(ILogger<AZLyricsClient> logger, AZLyricsOptions aZLyricsOptions)
        {
            _logger = logger;
            Ensure.ArgumentNotNull(aZLyricsOptions, nameof(aZLyricsOptions));
            Options = aZLyricsOptions;

            Parser = new AZLyricsParser();
            WebClient = new HtmlAgilityWebClient();
        }

        public AZLyricsClient(ILogger<AZLyricsClient> logger, IOptionsSnapshot<AZLyricsOptions> aZLyricsOptions) 
            : this(logger, aZLyricsOptions.Value)
        {
            Ensure.ArgumentNotNull(aZLyricsOptions, nameof(aZLyricsOptions));
        }

        public override AZLyricsOptions Options { get; }

        #region Sync

        protected override string SearchLyric(string artist, string song)
        {
            return SearchLyric(GetLyricUri(artist, song));
        }

        protected override string SearchLyric(Uri uri)
        {
            if (WebClient == null || Parser == null)
            {
                _logger?.LogError($"Please set up WebClient and Parser first");
                return null;
            }
            var text = WebClient.Load(uri);
            return PostProcessLyric(uri, text);
        }

        #endregion


        #region Async

        protected override async Task<string> SearchLyricAsync(string artist, string song)
        {
            return await SearchLyricAsync(GetLyricUri(artist, song));
        }

        protected override async Task<string> SearchLyricAsync(Uri uri)
        {
            if (WebClient == null || Parser == null)
            {
                _logger?.LogError($"Please set up WebClient and Parser first");
                return null;
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

        private string PostProcessLyric(Uri uri, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                _logger?.LogError($"Text is empty for {uri}");
                return null;
            }

            var startIndex = text.IndexOf(_lyricStart);
            var endIndex = text.IndexOf(_lyricEnd);
            if (startIndex <= 0 || endIndex <= 0)
            {
                _logger?.LogError($"Can't find lyrics for {uri}");
                return null;
            }
            return Parser.Parse(text.Substring(startIndex, endIndex - startIndex));
        }
    }
}
