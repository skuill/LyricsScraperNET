using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;

namespace LyricsScraperNET.Providers.AZLyrics
{
    public sealed class AZLyricsOptions : IExternalProviderOptions
    {
        public bool Enabled { get; set; }

        public ExternalProviderType ExternalProviderType => ExternalProviderType.AZLyrics;

        public const string ConfigurationSectionName = "AZLyricsOptions";

        public override bool Equals(object? obj)
        {
            return obj is AZLyricsOptions options &&
                   ExternalProviderType == options.ExternalProviderType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ExternalProviderType);
        }
    }
}
