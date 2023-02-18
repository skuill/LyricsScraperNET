using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;

namespace LyricsScraperNET.Providers.Genius
{
    public sealed class GeniusOptions : IExternalProviderOptions
    {
        public bool Enabled { get; set; }

        public string ApiKey { get; set; }

        public const string ConfigurationSectionName = "GeniusOptions";

        public ExternalProviderType ExternalProviderType => ExternalProviderType.Genius;

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
