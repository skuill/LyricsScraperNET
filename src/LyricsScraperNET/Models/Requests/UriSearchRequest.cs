namespace LyricsScraperNET.Models.Requests
{
    public record UriSearchRequest(Uri Uri) : SearchRequest
    {
        public UriSearchRequest(string uri) : this(new Uri(uri)) { }
    }
}
