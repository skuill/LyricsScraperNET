using LyricsScraperNET.Providers.Abstract;
using System;

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

        /// <summary>
        /// Maximum time one external provider gets to answer, for example "00:00:10". Null means the library default (10 seconds).
        /// </summary>
        TimeSpan? ProviderTimeout { get; }

        IExternalProviderOptions AZLyricsOptions { get; }

        IExternalProviderOptions GeniusOptions { get; }

        IExternalProviderOptions SongLyricsOptions { get; }

        IExternalProviderOptions KPopLyricsOptions { get; }

        IExternalProviderOptions LyricsFreakOptions { get; }

        IExternalProviderOptions LrclibOptions { get; }
    }
}
