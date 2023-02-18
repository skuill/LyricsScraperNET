using LyricsScraperNET.Providers.Models;

namespace LyricsScraperNET.Providers.Abstract
{
    public interface IExternalProviderOptions
    {
        ExternalProviderType ExternalProviderType { get; }

        bool Enabled { get; set; }

        /// <summary>
        /// If there are multiple external providers, then the search will start from the provider with the highest priority.
        /// </summary>
        int SearchPriority { get; set; }
    }
}
