using LyricsScraperNET.Models.Responses;

namespace LyricsScraperNET.Models.Requests
{
    /// <summary>
    /// Base class of <see cref="ArtistAndSongSearchRequest"/> and <see cref="UriSearchRequest"/>. Other request types are not supported:
    /// the validation member is internal, so the class cannot be derived from outside the library.
    /// </summary>
    public abstract class SearchRequest
    {
        /// <summary>
        /// Returns false with the code and a human readable message if the request cannot be searched.
        /// </summary>
        internal abstract bool IsValid(out SearchResultCode errorCode, out string errorMessage);
    }
}
