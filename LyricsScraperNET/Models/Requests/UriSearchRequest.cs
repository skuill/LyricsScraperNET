using System;

namespace LyricsScraperNET.Models.Requests
{
    public class UriSearchRequest : SearchRequest
    {
        public Uri Uri { get; }

        public UriSearchRequest(Uri uri)
        {
            Uri = uri;
        }

        public UriSearchRequest(string uri) : this(new Uri(uri))
        { }
    }
}
