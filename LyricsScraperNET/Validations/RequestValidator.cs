using LyricsScraperNET.Common;
using LyricsScraperNET.Extensions;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Providers;
using Microsoft.Extensions.Logging;

namespace LyricsScraperNET.Validations
{
    public sealed class RequestValidator : IRequestValidator
    {
        public bool IsValidSearchRequest(
            IProviderService providerService,
            SearchRequest searchRequest,
            out string errorMessage,
            out LogLevel logLevel)
        {
            logLevel = LogLevel.Error;

            if (searchRequest == null)
            {
                errorMessage = Constants.ResponseMessages.SearchRequestIsEmpty;
                return false;
            }

            var isValid = searchRequest.IsValid(out errorMessage);
            if (!isValid)
            {
                return false;
            }

            var providerType = searchRequest.GetProviderType();
            if (!providerType.IsNoneProviderType() && !providerService.IsProviderEnabled(providerType))
            {
                errorMessage = Constants.ResponseMessages.ExternalProviderForRequestNotSpecified;
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public bool IsValidClientConfiguration(
            IProviderService providerService,
            out string errorMessage,
            out LogLevel logLevel)
        {
            logLevel = LogLevel.Warning;

            if (!providerService.AnyAvailable())
            {
                errorMessage = Constants.ResponseMessages.ExternalProvidersListIsEmpty;
                return false;
            }

            if (!providerService.AnyEnabled())
            {
                errorMessage = Constants.ResponseMessages.ExternalProvidersAreDisabled;
                logLevel = LogLevel.Debug;
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
