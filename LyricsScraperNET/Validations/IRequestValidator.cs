using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers;
using Microsoft.Extensions.Logging;

namespace LyricsScraperNET.Validations
{
    internal interface IRequestValidator
    {
        bool IsValidSearchRequest(IProviderService providerService, SearchRequest searchRequest, out SearchResultCode errorCode, out string errorMessage, out LogLevel logLevel);

        bool IsValidClientConfiguration(IProviderService providerService, out SearchResultCode errorCode, out string errorMessage, out LogLevel logLevel);
    }
}
