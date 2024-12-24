using LyricsScraperNET.Providers.Models;

namespace LyricsScraperNET.Models.Responses
{
    /// <summary>
    /// Lyrics search result model.
    /// </summary>
    public class SearchResult
    {
        internal SearchResult()
        {
            LyricText = string.Empty;
            ExternalProviderType = ExternalProviderType.None;
            ResponseStatusCode = ResponseStatusCode.NoDataFound;
        }

        internal SearchResult(ExternalProviderType externalProviderType)
            : this()
        {
            ExternalProviderType = externalProviderType;
        }

        internal SearchResult(ExternalProviderType externalProviderType, ResponseStatusCode responseStatusCode)
            : this(externalProviderType)
        {
            ResponseStatusCode = responseStatusCode;
        }

        internal SearchResult(string lyricText, ExternalProviderType externalProviderType)
            : this(externalProviderType)
        {
            LyricText = lyricText;
            ResponseStatusCode = ResponseStatusCode.Success;
        }

        /// <summary>
        /// The text of the found lyrics. If the lyrics could not be found, an empty value is returned.
        /// </summary>
        public string LyricText { get; }

        /// <summary>
        /// The type of external provider for which the lyrics were found.
        /// </summary>
        public ExternalProviderType ExternalProviderType { get; }

        /// <summary>
        /// Search result status code.
        /// </summary>
        public ResponseStatusCode ResponseStatusCode { get; internal set; } = ResponseStatusCode.Success;

        /// <summary>
        /// A message that may contain additional information in case of problems with the search.
        /// </summary>
        public string ResponseMessage { get; internal set; } = string.Empty;

        /// <summary>
        /// The flag indicates that the search results are for music only, without text.
        /// </summary>
        public bool Instrumental { get; internal set; } = false;

        /// <summary>
        /// Returns true if the field <seealso cref="LyricText"/> is empty.
        /// </summary>
        public bool IsEmpty() => string.IsNullOrWhiteSpace(LyricText);

        /// <summary>
        /// Represents an empty search result.
        /// </summary>
        public static SearchResult Empty { get; } = new SearchResult
        {
            ResponseStatusCode = ResponseStatusCode.NoDataFound,
            Instrumental = false
        };
    }
}
