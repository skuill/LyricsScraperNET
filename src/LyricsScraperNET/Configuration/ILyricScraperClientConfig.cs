using LyricsScraperNET.External.Abstract;

namespace LyricsScraperNET.Configuration
{
    public interface ILyricScraperClientConfig
    {
        /// <summary>
        /// Check if any external client options is enabled
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Check if any external client options is exist
        /// </summary>
        bool IsExternalClientOptionsExists { get; }

        IList<IExternalServiceClientOptions> ExternalServiceClientOptions { get; set; }

        void AddClientOptions(IExternalServiceClientOptions externalServiceClientOption);
    }
}
