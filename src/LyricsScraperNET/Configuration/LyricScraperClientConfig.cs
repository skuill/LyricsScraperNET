using LyricsScraperNET.External.Abstract;
using LyricsScraperNET.External.AZLyrics;
using LyricsScraperNET.External.Genius;

namespace LyricsScraperNET.Configuration
{
    public sealed class LyricScraperClientConfig : ILyricScraperClientConfig
    {
        public const string ConfigurationSectionName = "LyricScraperClient";

        public IExternalServiceClientOptions AZLyricsOptions { get; set; } = new AZLyricsOptions();

        public IExternalServiceClientOptions GeniusOptions { get; set; } = new GeniusOptions();

        public bool IsEnabled => AZLyricsOptions.Enabled || GeniusOptions.Enabled;
    }
}
