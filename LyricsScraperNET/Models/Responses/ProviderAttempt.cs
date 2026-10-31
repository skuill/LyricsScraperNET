using LyricsScraperNET.Providers.Models;
using System;

namespace LyricsScraperNET.Models.Responses
{
    /// <summary>
    /// What one provider did during a search: see <see cref="SearchResult.ProviderAttempts"/>.
    /// </summary>
    public sealed class ProviderAttempt
    {
        internal ProviderAttempt(ExternalProviderType providerType, ProviderAttemptOutcome outcome, TimeSpan duration, string? message = null)
        {
            ProviderType = providerType;
            Outcome = outcome;
            Duration = duration;
            Message = message ?? string.Empty;
        }

        /// <summary>
        /// The provider.
        /// </summary>
        public ExternalProviderType ProviderType { get; }

        /// <summary>
        /// How the provider ended.
        /// </summary>
        public ProviderAttemptOutcome Outcome { get; }

        /// <summary>
        /// Time from the start of the provider until its outcome was known.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// A short human readable reason, for example the HTTP status code of a failed request. Empty if there is nothing to add.
        /// </summary>
        public string Message { get; }

        public override string ToString()
            => string.IsNullOrEmpty(Message)
                ? $"{ProviderType}: {Outcome} ({Duration.TotalMilliseconds:0} ms)"
                : $"{ProviderType}: {Outcome} ({Duration.TotalMilliseconds:0} ms): {Message}";
    }
}
