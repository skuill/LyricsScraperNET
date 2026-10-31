using LyricsScraperNET.Common;
using System;

namespace LyricsScraperNET.Models.Responses
{
    /// <summary>
    /// Search response status code list.
    /// Obsolete: use <see cref="SearchResultCode"/> (<see cref="SearchResult.Code"/>). Removed in 4.0.0.
    /// </summary>
    [Obsolete(Obsoletions.ResponseStatusCode.Message, DiagnosticId = Obsoletions.ResponseStatusCode.DiagnosticId, UrlFormat = Obsoletions.SharedUrlFormat)]
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
        BadRequest = 4,

        /// <summary>
        /// Lyrics are unavailable due to regional restrictions. Never returned since the LyricFind provider was removed in 3.0.0.
        /// </summary>
        RegionRestricted = 5
    }
}
