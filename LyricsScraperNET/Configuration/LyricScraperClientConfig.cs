using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Genius;
using LyricsScraperNET.Providers.KPopLyrics;
using LyricsScraperNET.Providers.Lrclib;
using LyricsScraperNET.Providers.LyricsFreak;
using LyricsScraperNET.Providers.SongLyrics;
using System.Text.Json.Serialization;

namespace LyricsScraperNET.Configuration
{
    public sealed class LyricScraperClientConfig : ILyricScraperClientConfig
    {
        [JsonIgnore]
        public const string ConfigurationSectionName = "LyricScraperClient";

        public IExternalProviderOptions AZLyricsOptions { get; set; } = new AZLyricsOptions();

        public IExternalProviderOptions GeniusOptions { get; set; } = new GeniusOptions();

        public IExternalProviderOptions SongLyricsOptions { get; set; } = new SongLyricsOptions();

        public IExternalProviderOptions KPopLyricsOptions { get; set; } = new KPopLyricsOptions();

        public IExternalProviderOptions LyricsFreakOptions { get; set; } = new LyricsFreakOptions();

        public IExternalProviderOptions LrclibOptions { get; set; } = new LrclibOptions();

        /// <inheritdoc />
        public bool UseParallelSearch { get; set; } = false;

        /// <inheritdoc />
        public bool IsEnabled => AZLyricsOptions.Enabled
            || GeniusOptions.Enabled
            || SongLyricsOptions.Enabled
            || KPopLyricsOptions.Enabled
            || LyricsFreakOptions.Enabled
            || LrclibOptions.Enabled;
    }
}
