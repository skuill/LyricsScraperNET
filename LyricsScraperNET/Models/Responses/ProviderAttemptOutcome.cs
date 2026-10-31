using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Abstract;
using LyricsScraperNET.Models.Serialization;
using System.Text.Json.Serialization;
using System;

namespace LyricsScraperNET.Models.Responses
{
    /// <summary>
    /// How one provider ended its part of a search. Compare with the static properties, or use <see cref="Values"/> in a <c>switch</c>.
    /// New outcomes may be added in minor versions: handle unknown outcomes. Written to JSON as a string, for example <c>"Timeout"</c>.
    /// </summary>
    [JsonConverter(typeof(ExtensibleEnumJsonConverter<ProviderAttemptOutcome>))]
    public readonly struct ProviderAttemptOutcome : IEquatable<ProviderAttemptOutcome>, IExtensibleEnum<ProviderAttemptOutcome>
    {
        private readonly string? _value;

        /// <summary>
        /// Creates an outcome from its string value, for example when reading it back from JSON.
        /// </summary>
        public ProviderAttemptOutcome(string value)
        {
            Ensure.ArgumentNotNullOrEmptyString(value, nameof(value));
            _value = value;
        }

        /// <summary>
        /// The string value. <c>default(ProviderAttemptOutcome)</c> is <see cref="None"/>.
        /// </summary>
        public string Value => _value ?? Values.None;

        /// <summary>Not set.</summary>
        public static ProviderAttemptOutcome None { get; } = new(Values.None);

        /// <summary>The provider found the lyrics.</summary>
        public static ProviderAttemptOutcome Found { get; } = new(Values.Found);

        /// <summary>The provider reports an instrumental song.</summary>
        public static ProviderAttemptOutcome Instrumental { get; } = new(Values.Instrumental);

        /// <summary>The provider answered and has no lyrics for the request.</summary>
        public static ProviderAttemptOutcome NotFound { get; } = new(Values.NotFound);

        /// <summary>The site could not be loaded: network error, unsuccessful status code (for example 403, 429, 503) or a timeout of the HTTP client.</summary>
        public static ProviderAttemptOutcome Failed { get; } = new(Values.Failed);

        /// <summary>The provider did not answer within <see cref="ILyricsScraperClient.ProviderTimeout"/>.</summary>
        public static ProviderAttemptOutcome Timeout { get; } = new(Values.Timeout);

        /// <summary>The provider threw an exception.</summary>
        public static ProviderAttemptOutcome Error { get; } = new(Values.Error);

        /// <summary>The provider was stopped: the search was canceled or another provider already gave the result.</summary>
        public static ProviderAttemptOutcome Canceled { get; } = new(Values.Canceled);

        /// <summary>
        /// True if the provider gave an answer: <see cref="Found"/>, <see cref="Instrumental"/> or <see cref="NotFound"/>.
        /// </summary>
        public bool IsAnswer => this == Found || this == Instrumental || this == NotFound;

        public bool Equals(ProviderAttemptOutcome other) => string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object? obj) => obj is ProviderAttemptOutcome other && Equals(other);

        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

        public override string ToString() => Value;

        public static bool operator ==(ProviderAttemptOutcome left, ProviderAttemptOutcome right) => left.Equals(right);

        public static bool operator !=(ProviderAttemptOutcome left, ProviderAttemptOutcome right) => !left.Equals(right);

        static ProviderAttemptOutcome IExtensibleEnum<ProviderAttemptOutcome>.Create(string value) => new(value);

        /// <summary>
        /// String values of the outcomes, as constants for <c>switch</c> statements.
        /// </summary>
        public static class Values
        {
            public const string None = "None";
            public const string Found = "Found";
            public const string Instrumental = "Instrumental";
            public const string NotFound = "NotFound";
            public const string Failed = "Failed";
            public const string Timeout = "Timeout";
            public const string Error = "Error";
            public const string Canceled = "Canceled";
        }
    }
}
