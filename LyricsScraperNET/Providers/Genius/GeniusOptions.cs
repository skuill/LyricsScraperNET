using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;

namespace LyricsScraperNET.Providers.Genius
{
    public sealed class GeniusOptions : IExternalProviderOptionsWithApiKey
    {
        public bool Enabled { get; set; }

        // Optional. Use to retrieve lyric url for provided artist and song.
        public string ApiKey { get; set; }

        public string ConfigurationSectionName { get; } = "GeniusOptions";

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
            unchecked
            {
                int hash = 17;
                if (!string.IsNullOrEmpty(ApiKey))
                    hash = (hash * 31) + ApiKey.GetHashCode();
                hash = (hash * 31) + ExternalProviderType.GetHashCode();
                return hash;
            }
        }
    }
}
