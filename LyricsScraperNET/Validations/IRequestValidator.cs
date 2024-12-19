using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Providers;
using Microsoft.Extensions.Logging;

namespace LyricsScraperNET.Validations
{
    public interface IRequestValidator
    {
        bool IsValidSearchRequest(IProviderService providerService, SearchRequest searchRequest, out string errorMessage, out LogLevel logLevel);
        bool IsValidClientConfiguration(IProviderService providerService, out string errorMessage, out LogLevel logLevel);
    }
}
