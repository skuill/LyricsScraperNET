using Genius.Models.Response;
using HtmlAgilityPack;
using LyricsScraperNET.Abstract;
using LyricsScraperNET.Network.Html;
using Microsoft.Extensions.Logging;

namespace LyricsScraperNET.Genius
{
    public sealed class GeniusClient : ExternalServiceClientBase
    {
        private readonly ILogger<GeniusClient> _logger;
        private readonly string _apiKey;

        private const string GeniusSearchQueryFormat = "{0} {1}";

        public GeniusClient(ILogger<GeniusClient> logger, string apiKey)
        {
            _logger = logger;
            _apiKey = apiKey;
            Parser = new GeniusParser();
            WebClient = new HtmlAgilityWebClient();
        }

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

        public override string SearchLyric(Uri uri)
        {
            HttpClient httpClient = new();
            var htmlPageBody = httpClient.GetStringAsync(uri).GetAwaiter().GetResult();

            return GetParsedLyricFromHtmlPageBody(htmlPageBody);
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

        private string GetSearchQuery(string artist, string song) 
            => string.Format(GeniusSearchQueryFormat, artist, song);

        public override string SearchLyric(string artist, string song)
        {
            var geniusClient = new global::Genius.GeniusClient(_apiKey);

            var searchQuery = GetSearchQuery(artist, song);
            var searchGeniusResponse = geniusClient.SearchClient.Search(searchQuery).GetAwaiter().GetResult();

            var lyricUrl = GetLyricUrlFromSearchResponse(searchGeniusResponse, artist, song);
            
            return !string.IsNullOrWhiteSpace(lyricUrl) 
                ? SearchLyric(new Uri(lyricUrl)) 
                : string.Empty;
        }

        public override async Task<string> SearchLyricAsync(Uri uri)
        {
            HttpClient httpClient = new();
            var htmlPageBody = await httpClient.GetStringAsync(uri);

            return GetParsedLyricFromHtmlPageBody(htmlPageBody);
        }

        public override async Task<string> SearchLyricAsync(string artist, string song)
        {
            var geniusClient = new global::Genius.GeniusClient(_apiKey);

            var searchQuery = GetSearchQuery(artist, song);
            var searchGeniusResponse = await geniusClient.SearchClient.Search(searchQuery);

            var lyricUrl = GetLyricUrlFromSearchResponse(searchGeniusResponse, artist, song);

            return !string.IsNullOrWhiteSpace(lyricUrl)
                ? await SearchLyricAsync(new Uri(lyricUrl))
                : string.Empty;
        }
    }
}
