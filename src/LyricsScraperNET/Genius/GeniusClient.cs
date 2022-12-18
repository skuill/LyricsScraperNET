using HtmlAgilityPack;
using LyricsScraper.Abstract;
using LyricsScraper.Network.Abstract;
using LyricsScraper.Network.Html;
using Microsoft.Extensions.Logging;

namespace LyricsScraper.Genius
{
    public class GeniusClient : LyricClientBase
    {
        private readonly ILogger<GeniusClient> _logger;
        private readonly string _apiKey; 

        public GeniusClient(ILogger<GeniusClient> logger, ILyricParser<string> parser, ILyricWebClient webClient, string apiKey)
        {
            _logger = logger;
            _apiKey = apiKey;
            Parser = new GeniusParser();
            WebClient = new HtmlAgilityWebClient();
        }

        public override string SearchLyric(Uri uri)
        {
            var htmlDocument = new HtmlDocument();
            HttpClient httpClient = new();
            htmlDocument.LoadHtml((httpClient.GetStringAsync(uri).GetAwaiter().GetResult()).Replace("https://ajax.googleapis.com/ajax/libs/jquery/2.1.4/jquery.min.js", ""));

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

        public override string SearchLyric(string artist, string song)
        {
            var artistAndSong = $"{artist} {song}";

            var geniusClient = new global::Genius.GeniusClient(_apiKey);
            var searchGeniusResult = geniusClient.SearchClient.Search(artistAndSong).GetAwaiter().GetResult();
            if (searchGeniusResult.Meta.Status != 200)
            {
                _logger.LogError($"Can't find any information about artist {artist} and song {song}. Code: {searchGeniusResult.Meta.Status}. Message: {searchGeniusResult.Meta.Message}");
                return null;
            }
            var artistAndSongHit = searchGeniusResult.Response.Hits.FirstOrDefault(x => string.Equals(x.Result.PrimaryArtist.Name, artist, StringComparison.OrdinalIgnoreCase));

            if (artistAndSongHit == null || artistAndSongHit.Result == null)
            {
                _logger.LogError($"Can't find artist {artist} and song {song} hit.");
                return null;
            }

            _logger?.LogDebug($"Genius artist and song url: {artistAndSongHit.Result.Url}");

            // https://genius.com/Parkway-drive-wishing-wells-lyrics
            var lyricUrl = artistAndSongHit.Result.Url;

            return SearchLyric(new Uri(lyricUrl));
        }

        public override Task<string> SearchLyricAsync(Uri uri)
        {
            throw new NotImplementedException();
        }

        public override Task<string> SearchLyricAsync(string artist, string song)
        {
            throw new NotImplementedException();
        }
    }
}
