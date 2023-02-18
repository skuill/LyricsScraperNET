using Genius;
using Genius.Models.Response;
using HtmlAgilityPack;
using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network.Html;
using LyricsScraperNET.Providers.Abstract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.Genius
{
    public sealed class GeniusProvider : ExternalProviderBase
    {
        private readonly ILogger<GeniusProvider> _logger;

        // Format: "artist song". Example: "Parkway Drive Carrion".
        private const string GeniusSearchQueryFormat = "{0} {1}";
        private const string GeniusApiSearchFormat = "https://genius.com/api/search?q={0}";

        public GeniusProvider()
        {
            Parser = new GeniusParser();
            WebClient = new HtmlAgilityWebClient();
            Options = new GeniusOptions() { Enabled = true };
        }

        public GeniusProvider(ILogger<GeniusProvider> logger, GeniusOptions geniusOptions) : this()
        {
            _logger = logger;
            Ensure.ArgumentNotNull(geniusOptions, nameof(geniusOptions));
            Options = geniusOptions;
        }

        public GeniusProvider(ILogger<GeniusProvider> logger, IOptionsSnapshot<GeniusOptions> geniusOptions)
            : this(logger, geniusOptions.Value)
        {
            Ensure.ArgumentNotNull(geniusOptions, nameof(geniusOptions));
        }

        public override IExternalProviderOptions Options { get; }

        private bool TryGetApiKeyFromOptions(out string apiKey)
        {
            apiKey = string.Empty;
            var geniusOptions = Options as GeniusOptions;

            if (geniusOptions == null || string.IsNullOrWhiteSpace(geniusOptions.ApiKey))
            {
                return false;
            }
            apiKey = geniusOptions.ApiKey;
            return true;
        }

        #region Sync

        protected override SearchResult SearchLyric(Uri uri)
        {
            HttpClient httpClient = new();
            var htmlPageBody = httpClient.GetStringAsync(uri).GetAwaiter().GetResult();

            return new SearchResult(GetParsedLyricFromHtmlPageBody(htmlPageBody), Models.ExternalProviderType.Genius);
        }

        private string GetLyricUrlWithoutApiKey(string artist, string song)
        {
            HttpClient httpClient = new();
            var htmlPageBody = httpClient.GetStringAsync(GetApiSearchUrl(artist, song)).GetAwaiter().GetResult();

            var parsedJsonResponse = JsonDocument.Parse(htmlPageBody);

            if (parsedJsonResponse.RootElement.TryGetProperty("response", out var responseJsonElement))
            {
                if (responseJsonElement.TryGetProperty("hits", out var hitsJsonElement))
                {
                    foreach (var hitJsonProperty in hitsJsonElement.EnumerateArray())
                    {
                        if (hitJsonProperty.TryGetProperty("result", out var resultJsonElement))
                        {
                            if (resultJsonElement.TryGetProperty("url", out var lyricUrl))
                                return lyricUrl.GetString();
                        }
                    }
                }
            }

            return string.Empty;
        }

        protected override SearchResult SearchLyric(string artist, string song)
        {
            string lyricUrl = string.Empty;

            if (TryGetApiKeyFromOptions(out string apiKey))
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
            HttpClient httpClient = new();
            var htmlPageBody = await httpClient.GetStringAsync(uri);

            return new SearchResult(GetParsedLyricFromHtmlPageBody(htmlPageBody), Models.ExternalProviderType.Genius);
        }

        protected override async Task<SearchResult> SearchLyricAsync(string artist, string song)
        {
            string lyricUrl = string.Empty;

            if (TryGetApiKeyFromOptions(out string apiKey))
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


        private string GetParsedLyricFromHtmlPageBody(string htmlPageBody)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlPageBody.Replace("https://ajax.googleapis.com/ajax/libs/jquery/2.1.4/jquery.min.js", ""));

            var fragmentNodes = htmlDocument.DocumentNode.SelectNodes("//a[contains(@class, 'ReferentFragmentVariantdesktop')]");
            if (fragmentNodes != null)
                foreach (HtmlNode node in fragmentNodes)
                    node.ParentNode.ReplaceChild(htmlDocument.CreateTextNode(node.ChildNodes[0].InnerHtml), node);
            var spanNodes = htmlDocument.DocumentNode.SelectNodes("//span");
            if (spanNodes != null)
                foreach (HtmlNode node in spanNodes)
                    node.Remove();

            var lyricNodes = htmlDocument.DocumentNode.SelectNodes("//div[@data-lyrics-container]");

            return Parser.Parse(string.Join("", lyricNodes.Select(Node => Node.InnerHtml)));
        }

        private string GetLyricUrlFromSearchResponse(SearchResponse searchResponse, string artist, string song)
        {
            if (searchResponse.Meta.Status != 200)
            {
                _logger.LogError($"Can't find any information about artist {artist} and song {song}. Code: {searchResponse.Meta.Status}. Message: {searchResponse.Meta.Message}");
                return string.Empty;
            }
            var artistAndSongHit = searchResponse.Response.Hits.FirstOrDefault(x => string.Equals(x.Result.PrimaryArtist.Name, artist, StringComparison.OrdinalIgnoreCase));

            if (artistAndSongHit == null || artistAndSongHit.Result == null)
            {
                _logger.LogError($"Can't find artist {artist} and song {song} hit.");
                return string.Empty;
            }

            _logger?.LogDebug($"Genius artist and song url: {artistAndSongHit.Result.Url}");

            // https://genius.com/Parkway-drive-wishing-wells-lyrics
            return artistAndSongHit.Result.Url;
        }

        private string GetApiSearchQuery(string artist, string song)
            => string.Format(GeniusSearchQueryFormat, artist, song);

        private string GetApiSearchUrl(string artist, string song)
            => string.Format(GeniusApiSearchFormat, GetApiSearchQuery(artist, song));
    }
}
