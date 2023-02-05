using LyricsScraperNET.External.Abstract;
using LyricsScraperNET.Helpers;

namespace LyricsScraperNET.Configuration
{
    public sealed class LyricScraperClientConfig : ILyricScraperClientConfig
    {
        public const string ConfigurationSectionName = "LyricScraperClient";

        public LyricScraperClientConfig (IEnumerable<IExternalServiceClientOptions> externalServiceClientOptions) 
        {
            Ensure.ArgumentNotNullOrEmptyList(externalServiceClientOptions, nameof(externalServiceClientOptions));
            ExternalServiceClientOptions = externalServiceClientOptions.ToList();
        }

        public IList<IExternalServiceClientOptions> ExternalServiceClientOptions { get; set; }

        public bool IsEnabled => ExternalServiceClientOptions != null && ExternalServiceClientOptions.Any(x => x.Enabled);

        public bool IsExternalClientOptionsExists => ExternalServiceClientOptions != null && ExternalServiceClientOptions.Any();

        public void AddClientOptions(IExternalServiceClientOptions externalServiceClientOption)
        {
            if (!IsExternalClientOptionsExists)
            {
                ExternalServiceClientOptions = new List<IExternalServiceClientOptions>();
            }

            if (ExternalServiceClientOptions != null && !ExternalServiceClientOptions.Contains(externalServiceClientOption))
            {
                ExternalServiceClientOptions.Add(externalServiceClientOption);
            }
        }
    }
}
