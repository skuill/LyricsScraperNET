using LyricsScraperNET.Common;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;

namespace LyricsScraperNET.Providers.KPopLyrics
{
    public class KPopLyricsOptions : IExternalProviderOptions
    {
        public ExternalProviderType ExternalProviderType => ExternalProviderType.KPopLyrics;

        public bool Enabled { get; set; }

        public int SearchPriority { get; set; } = Constants.ProvidersSearchPriorities[ExternalProviderType.KPopLyrics];

        public string ConfigurationSectionName => "KPopLyricsOptions";

        public override bool Equals(object? obj)
        {
            return obj is KPopLyricsOptions options &&
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