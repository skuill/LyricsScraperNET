using LyricsScraperNET.Common;
using LyricsScraperNET.Network.Models;
using LyricsScraperNET.Network;
using LyricsScraperNET.Providers.Models;
using System.Collections.Generic;
using System;

namespace LyricsScraperNET.Models.Responses
{
    /// <summary>
    /// Lyrics search result model.
    /// </summary>
    public class SearchResult
    {
        internal SearchResult()
            : this(ExternalProviderType.None)
        {
        }

        internal SearchResult(ExternalProviderType externalProviderType)
            : this(string.Empty, externalProviderType)
        {
        }

        /// <summary>
        /// <see cref="SearchResultCode.Found"/> if <paramref name="lyricText"/> is not empty, otherwise <see cref="SearchResultCode.NotFound"/>.
        /// </summary>
        internal SearchResult(string lyricText, ExternalProviderType externalProviderType)
            : this(lyricText, externalProviderType, string.IsNullOrWhiteSpace(lyricText) ? SearchResultCode.NotFound : SearchResultCode.Found)
        {
        }

        internal SearchResult(string lyricText, ExternalProviderType externalProviderType, SearchResultCode code)
        {
            LyricText = lyricText ?? string.Empty;
            ExternalProviderType = externalProviderType;
            Code = code;
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
        /// Why the search ended, for example <see cref="SearchResultCode.Found"/>, <see cref="SearchResultCode.NotFound"/>
        /// or <see cref="SearchResultCode.AllProvidersFailed"/>.
        /// </summary>
        public SearchResultCode Code { get; internal set; }

        /// <summary>
        /// Search result status code, derived from <see cref="Code"/>.
        /// </summary>
        [Obsolete(Obsoletions.ResponseStatusCode.Message, DiagnosticId = Obsoletions.ResponseStatusCode.DiagnosticId, UrlFormat = Obsoletions.SharedUrlFormat)]
#pragma warning disable LSNET0002 // The obsolete property returns the obsolete enum.
        public ResponseStatusCode ResponseStatusCode => ToResponseStatusCode(Code);
#pragma warning restore LSNET0002

        /// <summary>
        /// A human readable message about the result, for example why nothing was found. Use <see cref="Code"/> in code.
        /// </summary>
        public string ResponseMessage { get; internal set; } = string.Empty;

        /// <summary>
        /// The flag indicates that the search results are for music only, without text (<see cref="SearchResultCode.Instrumental"/>).
        /// </summary>
        public bool Instrumental => Code == SearchResultCode.Instrumental;

        /// <summary>
        /// What each started provider did, in the order their outcomes became known. Filled by <see cref="ILyricsScraperClient"/>;
        /// empty for a result returned by a provider directly.
        /// </summary>
        public IReadOnlyList<ProviderAttempt> ProviderAttempts { get; internal set; } = Array.Empty<ProviderAttempt>();

        /// <summary>
        /// The last failed page load of the provider call that produced this result, if the provider found nothing.
        /// The client turns it into <see cref="ProviderAttemptOutcome.Failed"/>.
        /// </summary>
        internal WebLoadResult? LoadFailure { get; set; }

        /// <summary>
        /// Returns true if the field <seealso cref="LyricText"/> is empty.
        /// </summary>
        public bool IsEmpty() => string.IsNullOrWhiteSpace(LyricText);

        /// <summary>
        /// A new empty result with <see cref="SearchResultCode.NotFound"/>. Each call returns a new instance.
        /// </summary>
        public static SearchResult Empty => new SearchResult();

        /// <summary>
        /// A copy with the given provider attempts. The result returned by a provider is not changed.
        /// </summary>
        internal SearchResult WithProviderAttempts(IReadOnlyList<ProviderAttempt> providerAttempts)
            => new SearchResult(LyricText, ExternalProviderType, Code)
            {
                ResponseMessage = ResponseMessage,
                ProviderAttempts = providerAttempts,
            };

#pragma warning disable LSNET0002 // Mapping to the obsolete enum, removed in 4.0.0 together with it.
        internal static ResponseStatusCode ToResponseStatusCode(SearchResultCode code)
        {
            if (code.IsSuccess)
                return ResponseStatusCode.Success;
            if (code == SearchResultCode.AllProvidersFailed)
                return ResponseStatusCode.Error;
            if (code == SearchResultCode.EmptyRequest || code == SearchResultCode.EmptyArtistOrSong
                || code == SearchResultCode.EmptyUri || code == SearchResultCode.ProviderNotAvailable)
                return ResponseStatusCode.BadRequest;
            if (code == SearchResultCode.None)
                return ResponseStatusCode.None;

            // NotFound, NoProviders, AllProvidersDisabled and codes added later.
            return ResponseStatusCode.NoDataFound;
        }
#pragma warning restore LSNET0002
    }
}
