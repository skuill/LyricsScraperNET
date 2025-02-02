using LyricsScraperNET.Common;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;

namespace LyricsScraperNET.Providers.LyricsFreak
{
    public sealed class LyricsFreakOptions : IExternalProviderOptions
    {
        public ExternalProviderType ExternalProviderType => ExternalProviderType.LyricsFreak;

        public bool Enabled { get; set; }

        public int SearchPriority { get; set; } = Constants.ProvidersSearchPriorities[ExternalProviderType.LyricsFreak];

        public string ConfigurationSectionName { get; } = "LyricsFreakOptions";

        public override bool Equals(object? obj)
        {
            return obj is LyricsFreakOptions options &&
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
