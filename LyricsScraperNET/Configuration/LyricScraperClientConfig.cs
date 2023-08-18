using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Genius;
using LyricsScraperNET.Providers.Musixmatch;
using LyricsScraperNET.Providers.SongLyrics;

namespace LyricsScraperNET.Configuration
{
    public sealed class LyricScraperClientConfig : ILyricScraperClientConfig
    {
        public const string ConfigurationSectionName = "LyricScraperClient";

        public IExternalProviderOptions AZLyricsOptions { get; set; } = new AZLyricsOptions();

        public IExternalProviderOptions GeniusOptions { get; set; } = new GeniusOptions();

        public IExternalProviderOptions MusixmatchOptions { get; set; } = new MusixmatchOptions();

        public IExternalProviderOptions SongLyricsOptions { get; set; } = new SongLyricsOptions();

        public bool IsEnabled => AZLyricsOptions.Enabled
            || GeniusOptions.Enabled
            || MusixmatchOptions.Enabled
            || SongLyricsOptions.Enabled;
    }
}
