using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Abstract;
using LyricsScraperNET.Models.Serialization;
using System.Text.Json.Serialization;
using System;

namespace LyricsScraperNET.Models.Responses
{
    /// <summary>
    /// Why a search ended. Compare with the static properties (<c>result.Code == SearchResultCode.NotFound</c>),
    /// or use <see cref="Values"/> in a <c>switch</c>. New codes may be added in minor versions: handle unknown codes.
    /// Written to JSON as a string, for example <c>"NotFound"</c>.
    /// </summary>
    [JsonConverter(typeof(ExtensibleEnumJsonConverter<SearchResultCode>))]
    public readonly struct SearchResultCode : IEquatable<SearchResultCode>, IExtensibleEnum<SearchResultCode>
    {
        private readonly string? _value;

        /// <summary>
        /// Creates a code from its string value, for example when reading it back from JSON.
        /// </summary>
        public SearchResultCode(string value)
        {
            Ensure.ArgumentNotNullOrEmptyString(value, nameof(value));
            _value = value;
        }

        /// <summary>
        /// The string value. <c>default(SearchResultCode)</c> is <see cref="None"/>.
        /// </summary>
        public string Value => _value ?? Values.None;

        /// <summary>Not set.</summary>
        public static SearchResultCode None { get; } = new(Values.None);

        /// <summary>Lyrics were found.</summary>
        public static SearchResultCode Found { get; } = new(Values.Found);

        /// <summary>The song is instrumental: the search succeeded, the lyrics text is empty.</summary>
        public static SearchResultCode Instrumental { get; } = new(Values.Instrumental);

        /// <summary>At least one provider answered, none of them has the lyrics.</summary>
        public static SearchResultCode NotFound { get; } = new(Values.NotFound);

        /// <summary>
        /// No provider could answer (site unavailable, timeout or error): the lyrics may exist.
        /// See <see cref="SearchResult.ProviderAttempts"/> for the reason of each provider.
        /// </summary>
        public static SearchResultCode AllProvidersFailed { get; } = new(Values.AllProvidersFailed);

        /// <summary>The search request is null.</summary>
        public static SearchResultCode EmptyRequest { get; } = new(Values.EmptyRequest);

        /// <summary>The artist or the song of an artist and song request is empty.</summary>
        public static SearchResultCode EmptyArtistOrSong { get; } = new(Values.EmptyArtistOrSong);

        /// <summary>The URI of a URI request is empty.</summary>
        public static SearchResultCode EmptyUri { get; } = new(Values.EmptyUri);

        /// <summary>The provider named in the request is not added to the client or is disabled.</summary>
        public static SearchResultCode ProviderNotAvailable { get; } = new(Values.ProviderNotAvailable);

        /// <summary>The client has no providers.</summary>
        public static SearchResultCode NoProviders { get; } = new(Values.NoProviders);

        /// <summary>All providers of the client are disabled.</summary>
        public static SearchResultCode AllProvidersDisabled { get; } = new(Values.AllProvidersDisabled);

        /// <summary>
        /// True for <see cref="Found"/> and <see cref="Instrumental"/>.
        /// </summary>
        public bool IsSuccess => this == Found || this == Instrumental;

        /// <summary>
        /// True for codes caused by the request or the client setup, not by the providers:
        /// <see cref="EmptyRequest"/>, <see cref="EmptyArtistOrSong"/>, <see cref="EmptyUri"/>, <see cref="ProviderNotAvailable"/>,
        /// <see cref="NoProviders"/>, <see cref="AllProvidersDisabled"/>.
        /// </summary>
        public bool IsInvalidRequestOrSetup
            => this == EmptyRequest || this == EmptyArtistOrSong || this == EmptyUri
            || this == ProviderNotAvailable || this == NoProviders || this == AllProvidersDisabled;

        public bool Equals(SearchResultCode other) => string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object? obj) => obj is SearchResultCode other && Equals(other);

        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

        public override string ToString() => Value;

        public static bool operator ==(SearchResultCode left, SearchResultCode right) => left.Equals(right);

        public static bool operator !=(SearchResultCode left, SearchResultCode right) => !left.Equals(right);

        static SearchResultCode IExtensibleEnum<SearchResultCode>.Create(string value) => new(value);

        /// <summary>
        /// String values of the codes, as constants for <c>switch</c> statements: <c>case SearchResultCode.Values.NotFound:</c>.
        /// </summary>
        public static class Values
        {
            public const string None = "None";
            public const string Found = "Found";
            public const string Instrumental = "Instrumental";
            public const string NotFound = "NotFound";
            public const string AllProvidersFailed = "AllProvidersFailed";
            public const string EmptyRequest = "EmptyRequest";
            public const string EmptyArtistOrSong = "EmptyArtistOrSong";
            public const string EmptyUri = "EmptyUri";
            public const string ProviderNotAvailable = "ProviderNotAvailable";
            public const string NoProviders = "NoProviders";
            public const string AllProvidersDisabled = "AllProvidersDisabled";
        }
    }
}
