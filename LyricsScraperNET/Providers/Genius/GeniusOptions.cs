using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;
using System;

namespace LyricsScraperNET.Providers.Genius
{
    public sealed class GeniusOptions : IExternalProviderOptionsWithApiKey
    {
        public bool Enabled { get; set; }

        // Optional. Use to retrieve lyric url for provided artist and song.
        public string ApiKey { get; set; }

        public const string ConfigurationSectionName = "GeniusOptions";

        public ExternalProviderType ExternalProviderType => ExternalProviderType.Genius;

        public int SearchPriority { get; set; } = 1;

        public override bool Equals(object? obj)
        {
            return obj is GeniusOptions options &&
                   ApiKey == options.ApiKey &&
                   ExternalProviderType == options.ExternalProviderType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ApiKey, ExternalProviderType);
        }
    }
}
