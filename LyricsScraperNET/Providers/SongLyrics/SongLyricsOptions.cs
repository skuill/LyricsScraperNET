using LyricsScraperNET.Common;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;

namespace LyricsScraperNET.Providers.SongLyrics
{
    public sealed class SongLyricsOptions : IExternalProviderOptions
    {
        public bool Enabled { get; set; }

        public ExternalProviderType ExternalProviderType => ExternalProviderType.SongLyrics;

        public int SearchPriority { get; set; } = Constants.ProvidersSearchPriorities[ExternalProviderType.SongLyrics];

        public string ConfigurationSectionName { get; } = "SongLyricsOptions";

        public override bool Equals(object? obj)
        {
            return obj is SongLyricsOptions options &&
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
