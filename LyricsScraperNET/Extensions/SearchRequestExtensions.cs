using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Providers.Models;

namespace LyricsScraperNET.Extensions
{
    internal static class SearchRequestExtensions
    {
        public static ExternalProviderType GetProviderTypeFromRequest(this SearchRequest searchRequest)
        {
            switch (searchRequest)
            {
                case ArtistAndSongSearchRequest artistAndSongSearchRequest:
                    return artistAndSongSearchRequest.Provider;
                case UriSearchRequest uriSearchRequest:
                    return uriSearchRequest.Provider;
                default:
                    return ExternalProviderType.None;
            }
        }
    }
}
