namespace LyricsScraperNET.Models
{
    public record UriSearchRequest(Uri Uri): SearchRequest
    {
        public UriSearchRequest(string uri): this(new Uri(uri)) { }
    }
}
