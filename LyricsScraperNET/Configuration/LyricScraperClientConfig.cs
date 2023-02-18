using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Genius;

namespace LyricsScraperNET.Configuration
{
    public sealed class LyricScraperClientConfig : ILyricScraperClientConfig
    {
        public const string ConfigurationSectionName = "LyricScraperClient";

        public IExternalProviderOptions AZLyricsOptions { get; set; } = new AZLyricsOptions();

        public IExternalProviderOptions GeniusOptions { get; set; } = new GeniusOptions();

        public bool IsEnabled => AZLyricsOptions.Enabled || GeniusOptions.Enabled;
    }
}
