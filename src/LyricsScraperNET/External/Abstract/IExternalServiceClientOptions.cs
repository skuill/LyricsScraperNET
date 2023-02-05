using LyricsScraperNET.External.Models;

namespace LyricsScraperNET.External.Abstract
{
    public interface IExternalServiceClientOptions
    {
        ExternalServiceType ExternalServiceType { get; }

        bool Enabled { get; set; }
    }
}
