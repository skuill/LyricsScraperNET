using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;

namespace LyricsScraperNET.Providers.AZLyrics
{
    public sealed class AZLyricsOptions : IExternalProviderOptions
    {
        public bool Enabled { get; set; }

        public ExternalProviderType ExternalProviderType => ExternalProviderType.AZLyrics;

        public int SearchPriority { get; set; } = 4;

        public const string ConfigurationSectionName = "AZLyricsOptions";

        public override bool Equals(object? obj)
        {
            return obj is AZLyricsOptions options &&
                   ExternalProviderType == options.ExternalProviderType;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + ExternalProviderType.GetHashCode();
                return hash;
            }
        }
    }
}
