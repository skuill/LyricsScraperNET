using Genius;
using Genius.Models.Response;
using HtmlAgilityPack;
using LyricsScraperNET.Extensions;
using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network;
using LyricsScraperNET.Providers.Abstract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.Genius
{
    public sealed class GeniusProvider : ExternalProviderBase
    {
        private readonly ILogger<GeniusProvider> _logger;
        private readonly IExternalUriConverter _uriConverter;

        // Format: "artist song". Example: "Parkway Drive Carrion".
        private const string GeniusSearchQueryFormat = "{0} {1}";

        private const string _referentFragmentNodesXPath = "//a[contains(@class, 'ReferentFragmentVariantdesktop') or contains(@class, 'ReferentFragmentdesktop')]";
        private const string _lyricsContainerNodesXPath = "//div[@data-lyrics-container]";

        #region Constructors

        public GeniusProvider()
        {
            Parser = new GeniusParser();
            WebClient = new NetHttpClient();
            Options = new GeniusOptions() { Enabled = true };
            _uriConverter = new GeniusUriConverter();
        }

        public GeniusProvider(ILogger<GeniusProvider> logger, GeniusOptions options) : this()
        {
            _logger = logger;
            Ensure.ArgumentNotNull(options, nameof(options));
            Options = options;
        }

        public GeniusProvider(ILogger<GeniusProvider> logger, IOptionsSnapshot<GeniusOptions> options)
            : this(logger, options.Value)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        #endregion

        public override IExternalProviderOptions Options { get; }

        #region Sync

        protected override SearchResult SearchLyric(Uri uri)
        {
            var htmlPageBody = WebClient.Load(uri);

            return new SearchResult(GetParsedLyricFromHtmlPageBody(htmlPageBody), Models.ExternalProviderType.Genius);
        }

        protected override SearchResult SearchLyric(string artist, string song)
        {
            string lyricUrl = string.Empty;

            if (Options.TryGetApiKeyFromOptions(out var apiKey))
            {
                var geniusClient = new GeniusClient(apiKey);

                var searchQuery = GetApiSearchQuery(artist, song);
                var searchGeniusResponse = geniusClient.SearchClient.Search(searchQuery).GetAwaiter().GetResult();

                lyricUrl = GetLyricUrlFromSearchResponse(searchGeniusResponse, artist, song);
            }
            if (string.IsNullOrEmpty(lyricUrl))
            {
                lyricUrl = GetLyricUrlWithoutApiKey(artist, song);
            }

            return !string.IsNullOrWhiteSpace(lyricUrl)
                ? SearchLyric(new Uri(lyricUrl))
                : new SearchResult();
        }

        #endregion

        #region Async

        protected override async Task<SearchResult> SearchLyricAsync(Uri uri)
        {
            var htmlPageBody = await WebClient.LoadAsync(uri);

            return new SearchResult(GetParsedLyricFromHtmlPageBody(htmlPageBody), Models.ExternalProviderType.Genius);
        }

        protected override async Task<SearchResult> SearchLyricAsync(string artist, string song)
        {
            string lyricUrl = string.Empty;

            if (Options.TryGetApiKeyFromOptions(out var apiKey))
            {
                var geniusClient = new GeniusClient(apiKey);

                var searchQuery = GetApiSearchQuery(artist, song);
                var searchGeniusResponse = await geniusClient.SearchClient.Search(searchQuery);

                lyricUrl = GetLyricUrlFromSearchResponse(searchGeniusResponse, artist, song);
            }
            if (string.IsNullOrEmpty(lyricUrl))
            {
                lyricUrl = GetLyricUrlWithoutApiKey(artist, song);
            }

            return !string.IsNullOrWhiteSpace(lyricUrl)
                ? await SearchLyricAsync(new Uri(lyricUrl))
                : new SearchResult();
        }

        #endregion

        private string GetLyricUrlWithoutApiKey(string artist, string song)
        {
            var htmlPageBody = WebClient.Load(_uriConverter.GetLyricUri(artist, song));

            if (string.IsNullOrWhiteSpace(htmlPageBody))
                return string.Empty;

            var parsedJsonResponse = JsonDocument.Parse(htmlPageBody);

            if (!parsedJsonResponse.RootElement.TryGetProperty("response", out var responseJsonElement))
                return string.Empty;

            if (!responseJsonElement.TryGetProperty("hits", out var hitsJsonElement))
                return string.Empty;

            foreach (var hitJsonProperty in hitsJsonElement.EnumerateArray())
            {
                if (hitJsonProperty.TryGetProperty("result", out var resultJsonElement))
                {
                    if (resultJsonElement.TryGetProperty("url", out var lyricUrl))
                        return lyricUrl.GetString();
                }
            }

            return string.Empty;
        }

        private string GetParsedLyricFromHtmlPageBody(string htmlPageBody)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlPageBody.Replace("https://ajax.googleapis.com/ajax/libs/jquery/2.1.4/jquery.min.js", ""));

            var referentFragmentNodes = htmlDocument.DocumentNode.SelectNodes(_referentFragmentNodesXPath);
            if (referentFragmentNodes != null)
                foreach (HtmlNode fragmentNode in referentFragmentNodes)
                    fragmentNode.ParentNode.ReplaceChild(htmlDocument.CreateTextNode(fragmentNode.ChildNodes[0].InnerHtml), fragmentNode);
            var spanNodes = htmlDocument.DocumentNode.SelectNodes("//span");
            if (spanNodes != null)
                foreach (HtmlNode spanNode in spanNodes)
                    spanNode.Remove();

            var lyricNodes = htmlDocument.DocumentNode.SelectNodes(_lyricsContainerNodesXPath);

            return Parser.Parse(string.Join("", lyricNodes.Select(Node => Node.InnerHtml)));
        }

        private string GetLyricUrlFromSearchResponse(SearchResponse searchResponse, string artist, string song)
        {
            if (searchResponse.Meta.Status != 200)
            {
                _logger?.LogWarning($"Genius. Can't find any information about artist {artist} and song {song}. Code: {searchResponse.Meta.Status}. Message: {searchResponse.Meta.Message}");
                return string.Empty;
            }
            var artistAndSongHit = searchResponse.Response.Hits.FirstOrDefault(
                x => string.Equals(x.Result.PrimaryArtist.Name, artist, StringComparison.OrdinalIgnoreCase));

            if (artistAndSongHit == null || artistAndSongHit.Result == null)
            {
                _logger?.LogWarning($"Genius. Can't find artist {artist} and song {song} hit.");
                return string.Empty;
            }

            _logger?.LogDebug($"Genius. Artist and song url: {artistAndSongHit.Result.Url}");

            // Example: https://genius.com/Parkway-drive-wishing-wells-lyrics
            return artistAndSongHit.Result.Url;
        }

        private string GetApiSearchQuery(string artist, string song)
            => string.Format(GeniusSearchQueryFormat, artist, song);
    }
}
