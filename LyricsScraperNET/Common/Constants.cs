namespace LyricsScraperNET.Common
{
    internal static class Constants
    {
        internal static class ResponseMessages
        {
            internal static readonly string ExternalProvidersListIsEmpty = "Empty external providers list! Please set any external provider first";

            internal static readonly string ExternalProvidersAreDisabled = "All external providers are disabled. Searching lyrics is disabled";

            internal static readonly string NotFoundLyric = "Can't find lyric for the search request";

            internal static readonly string ExternalProviderForRequestNotSpecified = "The provider specified in the request is disabled or has not been configured for the client";

            internal static readonly string SearchRequestIsEmpty = "Search request is empty";

            internal static readonly string ArtistAndSongSearchRequestFieldsAreEmpty = "The ArtistAndSongSearchRequest not valid and contains one or more empty fields";

            internal static readonly string UriSearchRequestFieldsAreEmpty = "The UriSearchRequest not valid and contains one or more empty fields";
        }
    }
}
