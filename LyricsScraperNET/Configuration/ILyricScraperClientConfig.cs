using LyricsScraperNET.Providers.Abstract;

namespace LyricsScraperNET.Configuration
{
    public interface ILyricScraperClientConfig
    {
        /// <summary>
        /// Check if any external provider options is enabled
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Enable parallel search instead of sequential, 
        /// searching across all available external providers.
        /// </summary>
        bool UseParallelSearch { get; }

        IExternalProviderOptions AZLyricsOptions { get; }

        IExternalProviderOptions GeniusOptions { get; }

        IExternalProviderOptions MusixmatchOptions { get; }

        IExternalProviderOptions SongLyricsOptions { get; }

        IExternalProviderOptions LyricFindOptions { get; }
        IExternalProviderOptions KPopLyricsOptions { get; }
    }
}
