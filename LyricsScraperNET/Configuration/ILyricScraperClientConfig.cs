using LyricsScraperNET.Providers.Abstract;

namespace LyricsScraperNET.Configuration
{
    public interface ILyricScraperClientConfig
    {
        /// <summary>
        /// Check if any external provider options is enabled
        /// </summary>
        bool IsEnabled { get; }

        IExternalProviderOptions AZLyricsOptions { get; }

        IExternalProviderOptions GeniusOptions { get; }

        IExternalProviderOptions MusixmatchOptions { get; }
    }
}
