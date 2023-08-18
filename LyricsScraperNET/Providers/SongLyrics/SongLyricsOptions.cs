using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;
using System;

namespace LyricsScraperNET.Providers.SongLyrics
{
    public sealed class SongLyricsOptions : IExternalProviderOptions
    {
        public bool Enabled { get; set; }

        public ExternalProviderType ExternalProviderType => ExternalProviderType.SongLyrics;

        public int SearchPriority { get; set; } = 0;

        public const string ConfigurationSectionName = "SongLyricsOptions";

        public override bool Equals(object? obj)
        {
            return obj is SongLyricsOptions options &&
                   ExternalProviderType == options.ExternalProviderType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ExternalProviderType);
        }
    }
}
