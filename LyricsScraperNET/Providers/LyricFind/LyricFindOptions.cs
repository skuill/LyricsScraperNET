using LyricsScraperNET.Common;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;

namespace LyricsScraperNET.Providers.LyricFind
{
    public sealed class LyricFindOptions : IExternalProviderOptions
    {
        public bool Enabled { get; set; }

        public ExternalProviderType ExternalProviderType => ExternalProviderType.LyricFind;

        public int SearchPriority { get; set; } = Constants.ProvidersSearchPriorities[ExternalProviderType.LyricFind];

        public string ConfigurationSectionName { get; } = "LyricFindOptions";

        public override bool Equals(object? obj)
        {
            return obj is LyricFindOptions options &&
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
