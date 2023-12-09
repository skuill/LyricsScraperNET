using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;
using System;

namespace LyricsScraperNET.Providers.LyricFind
{
    public sealed class LyricFindOptions : IExternalProviderOptions
    {
        public bool Enabled { get; set; }

        public ExternalProviderType ExternalProviderType => ExternalProviderType.LyricFind;

        public int SearchPriority { get; set; } = 3;

        public const string ConfigurationSectionName = "LyricFindOptions";

        public override bool Equals(object? obj)
        {
            return obj is LyricFindOptions options &&
                   ExternalProviderType == options.ExternalProviderType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ExternalProviderType);
        }
    }
}
