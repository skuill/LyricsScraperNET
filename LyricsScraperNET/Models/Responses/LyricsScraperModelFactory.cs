using LyricsScraperNET.Providers.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LyricsScraperNET.Models.Responses
{
    /// <summary>
    /// Creates result models for tests of code that uses the library: their constructors are internal,
    /// so a fake <see cref="ILyricsScraperClient"/> can return any result.
    /// </summary>
    public static class LyricsScraperModelFactory
    {
        /// <summary>
        /// Creates a <see cref="Responses.SearchResult"/>.
        /// </summary>
        /// <param name="code">Why the search ended, for example <see cref="SearchResultCode.Found"/>.</param>
        /// <param name="lyricText">The lyrics, empty if nothing was found.</param>
        /// <param name="providerType">The provider that answered.</param>
        /// <param name="responseMessage">A human readable message.</param>
        /// <param name="providerAttempts">What each provider did, see <see cref="ProviderAttempt(ExternalProviderType, ProviderAttemptOutcome, TimeSpan, string)"/>.</param>
        public static SearchResult SearchResult(
            SearchResultCode code,
            string lyricText = "",
            ExternalProviderType providerType = ExternalProviderType.None,
            string responseMessage = "",
            IEnumerable<ProviderAttempt>? providerAttempts = null)
            => new SearchResult(lyricText, providerType, code)
            {
                ResponseMessage = responseMessage ?? string.Empty,
                ProviderAttempts = providerAttempts?.ToArray() ?? Array.Empty<ProviderAttempt>(),
            };

        /// <summary>
        /// Creates a <see cref="Responses.ProviderAttempt"/>.
        /// </summary>
        public static ProviderAttempt ProviderAttempt(
            ExternalProviderType providerType,
            ProviderAttemptOutcome outcome,
            TimeSpan duration = default,
            string message = "")
            => new ProviderAttempt(providerType, outcome, duration, message);
    }
}
