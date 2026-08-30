using LyricsScraperNET.Common;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;

namespace LyricsScraperNET.Providers.Lrclib
{
    public sealed class LrclibOptions : IExternalProviderOptions
    {
        public bool Enabled { get; set; }

        public ExternalProviderType ExternalProviderType => ExternalProviderType.Lrclib;

        public int SearchPriority { get; set; } = Constants.ProvidersSearchPriorities[ExternalProviderType.Lrclib];

        public string ConfigurationSectionName { get; } = "LrclibOptions";

        public override bool Equals(object? obj)
        {
            return obj is LrclibOptions options &&
                   ExternalProviderType == options.ExternalProviderType;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(ExternalProviderType);
        }
    }
}
