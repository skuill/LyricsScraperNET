using LyricsScraperNET.External.Abstract;

namespace LyricsScraperNET.Configuration
{
    public interface ILyricScraperClientConfig
    {
        /// <summary>
        /// Check if any external client options is enabled
        /// </summary>
        bool IsEnabled { get; }

        IExternalServiceClientOptions AZLyricsOptions { get; }

        IExternalServiceClientOptions GeniusOptions { get; }
    }
}
