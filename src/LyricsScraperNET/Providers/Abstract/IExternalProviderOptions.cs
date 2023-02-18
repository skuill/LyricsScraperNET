using LyricsScraperNET.Providers.Models;

namespace LyricsScraperNET.Providers.Abstract
{
    public interface IExternalProviderOptions
    {
        ExternalProviderType ExternalProviderType { get; }

        bool Enabled { get; set; }
    }
}
