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
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.SongLyrics
{
    public sealed class SongLyricsProvider : ExternalProviderBase
    {
        private readonly ILogger<SongLyricsProvider> _logger;

        // 0 - artist, 1 - song
        private const string SongLyricsUriPathFormat = "https://www.songlyrics.com/{0}/{1}-lyrics/";

        private const string _lyricsContainerNodesXPath = "//p[contains(@id, 'songLyricsDiv')]";

        private const string NotExistLyricPattern = "We do not have the lyrics for (.*) yet.";

        public SongLyricsProvider()
        {
            WebClient = new NetHttpClient();
            Options = new SongLyricsOptions() { Enabled = true };
        }

        public SongLyricsProvider(ILogger<SongLyricsProvider> logger, SongLyricsOptions songLyricsOptions) : this()
        {
            _logger = logger;
            Ensure.ArgumentNotNull(songLyricsOptions, nameof(songLyricsOptions));
            Options = songLyricsOptions;
        }

        public SongLyricsProvider(ILogger<SongLyricsProvider> logger, IOptionsSnapshot<SongLyricsOptions> songLyricsOptions)
            : this(logger, songLyricsOptions.Value)
        {
            Ensure.ArgumentNotNull(songLyricsOptions, nameof(songLyricsOptions));
        }

        public override IExternalProviderOptions Options { get; }

        #region Sync

        protected override SearchResult SearchLyric(string artist, string song)
        {
            return SearchLyric(GetLyricUri(artist, song));
        }

        protected override SearchResult SearchLyric(Uri uri)
        {
            if (WebClient == null)
            {
                _logger?.LogError($"SongLyrics. Please set up WebClient first");
                return new SearchResult();
            }
            var htmlPageBody = WebClient.Load(uri);
            return GetParsedLyricFromHtmlPageBody(uri, htmlPageBody);
        }

        #endregion


        #region Async

        protected override async Task<SearchResult> SearchLyricAsync(string artist, string song)
        {
            return await SearchLyricAsync(GetLyricUri(artist, song));
        }

        protected override async Task<SearchResult> SearchLyricAsync(Uri uri)
        {
            if (WebClient == null)
            {
                _logger?.LogError($"SongLyrics. Please set up WebClient first");
                return new SearchResult();
            }
            var htmlPageBody = await WebClient.LoadAsync(uri);
            return GetParsedLyricFromHtmlPageBody(uri, htmlPageBody);
        }

        #endregion


        private Uri GetLyricUri(string artist, string song)
        {
            // Attack Attack! - I Swear I'll Change -> https://www.songlyrics.com/attack-attack!/i-swear-i-ll-change-lyrics/
            // Against Me! - Stop! -> https://www.songlyrics.com/against-me!/stop!-lyrics/
            // The Devil Wears Prada - You Can't Spell Crap Without "C" -> https://www.songlyrics.com/the-devil-wears-prada/you-can-t-spell-crap-without-c-lyrics/
            var artistFormatted = artist.ToLowerInvariant().СonvertToDashedFormat();
            var songFormatted = song.ToLowerInvariant().СonvertToDashedFormat();
            return new Uri(string.Format(SongLyricsUriPathFormat, artistFormatted, songFormatted));
        }

        private SearchResult GetParsedLyricFromHtmlPageBody(Uri uri, string htmlPageBody)
        {
            if (string.IsNullOrEmpty(htmlPageBody))
            {
                _logger?.LogError($"SongLyrics. Text is empty for {uri}");
                return new SearchResult();
            }

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlPageBody);

            var lyricsContainerNode = htmlDocument.DocumentNode.SelectSingleNode(_lyricsContainerNodesXPath);

            if (lyricsContainerNode == null)
            {
                _logger?.LogError($"SongLyrics. Can't find lyrics for {uri}");
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
