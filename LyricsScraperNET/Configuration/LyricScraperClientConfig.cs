﻿using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Genius;
using LyricsScraperNET.Providers.LyricFind;
using LyricsScraperNET.Providers.LyricsFreak;
using LyricsScraperNET.Providers.Musixmatch;
using LyricsScraperNET.Providers.SongLyrics;
using System.Text.Json.Serialization;
using LyricsScraperNET.Providers.KPopLyrics;

namespace LyricsScraperNET.Configuration
{
    public sealed class LyricScraperClientConfig : ILyricScraperClientConfig
    {
        [JsonIgnore]
        public const string ConfigurationSectionName = "LyricScraperClient";

        public IExternalProviderOptions AZLyricsOptions { get; set; } = new AZLyricsOptions();

        public IExternalProviderOptions GeniusOptions { get; set; } = new GeniusOptions();

        public IExternalProviderOptions MusixmatchOptions { get; set; } = new MusixmatchOptions();

        public IExternalProviderOptions SongLyricsOptions { get; set; } = new SongLyricsOptions();

        public IExternalProviderOptions LyricFindOptions { get; set; } = new LyricFindOptions();

        public IExternalProviderOptions KPopLyricsOptions { get; set; } = new KPopLyricsOptions();

        public IExternalProviderOptions LyricsFreakOptions { get; set; } = new LyricsFreakOptions();

        /// <inheritdoc />
        public bool UseParallelSearch { get; set; } = false;

        /// <inheritdoc />
        public bool IsEnabled => AZLyricsOptions.Enabled
            || GeniusOptions.Enabled
            || MusixmatchOptions.Enabled
            || SongLyricsOptions.Enabled
            || LyricFindOptions.Enabled
            || KPopLyricsOptions.Enabled
            || LyricsFreakOptions.Enabled;
    }
}
