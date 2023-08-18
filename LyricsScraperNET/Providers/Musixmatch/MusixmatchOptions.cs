using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;
using System;

namespace LyricsScraperNET.Providers.Musixmatch
{
    public sealed class MusixmatchOptions : IExternalProviderOptionsWithApiKey
    {
        public bool Enabled { get; set; }

        // Optional. Without using the API, a token with restrictions on the search for lyrics will be generated.
        public string ApiKey { get; set; }

        public const string ConfigurationSectionName = "MusixmatchOptions";

        public ExternalProviderType ExternalProviderType => ExternalProviderType.Musixmatch;

        public int SearchPriority { get; set; } = 2;

        public override bool Equals(object? obj)
        {
            return obj is MusixmatchOptions options &&
                   ApiKey == options.ApiKey &&
                   ExternalProviderType == options.ExternalProviderType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ApiKey, ExternalProviderType);
        }
    }
}
