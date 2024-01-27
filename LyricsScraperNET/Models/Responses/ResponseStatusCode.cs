namespace LyricsScraperNET.Models.Responses
{
    /// <summary>
    /// Search response status code list.
    /// </summary>
    public enum ResponseStatusCode
    {
        /// <summary>
        /// Not specified status code.
        /// </summary>
        None = 0,

        /// <summary>
        /// An error occurred during the search.
        /// </summary>
        Error = 1,

        /// <summary>
        /// The lyrics was found in one of the available providers.
        /// </summary>
        Success = 2,

        /// <summary>
        /// The lyrics could not be found in one of the available providers.
        /// </summary>
        NoDataFound = 3,

        /// <summary>
        /// The search request is incorrect or contains malformed data.
        /// </summary>
        BadRequest = 4
    }
}
