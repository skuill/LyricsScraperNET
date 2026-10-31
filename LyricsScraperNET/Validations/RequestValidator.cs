using LyricsScraperNET.Common;
using LyricsScraperNET.Extensions;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers;
using Microsoft.Extensions.Logging;

namespace LyricsScraperNET.Validations
{
    internal sealed class RequestValidator : IRequestValidator
    {
        public bool IsValidSearchRequest(
            IProviderService providerService,
            SearchRequest searchRequest,
            out SearchResultCode errorCode,
            out string errorMessage,
            out LogLevel logLevel)
        {
            logLevel = LogLevel.Error;

            if (searchRequest == null)
            {
                errorCode = SearchResultCode.EmptyRequest;
                errorMessage = Constants.ResponseMessages.SearchRequestIsEmpty;
                return false;
            }

            if (!searchRequest.IsValid(out errorCode, out errorMessage))
            {
                return false;
            }

            var providerType = searchRequest.GetProviderType();
            if (!providerType.IsNoneProviderType() && !providerService.IsProviderEnabled(providerType))
            {
                errorCode = SearchResultCode.ProviderNotAvailable;
                errorMessage = Constants.ResponseMessages.ExternalProviderForRequestNotSpecified;
                return false;
            }

            errorCode = SearchResultCode.None;
            errorMessage = string.Empty;
            return true;
        }

        public bool IsValidClientConfiguration(
            IProviderService providerService,
            out SearchResultCode errorCode,
            out string errorMessage,
            out LogLevel logLevel)
        {
            logLevel = LogLevel.Warning;

            if (!providerService.AnyAvailable())
            {
                errorCode = SearchResultCode.NoProviders;
                errorMessage = Constants.ResponseMessages.ExternalProvidersListIsEmpty;
                return false;
            }

            if (!providerService.AnyEnabled())
            {
                errorCode = SearchResultCode.AllProvidersDisabled;
                errorMessage = Constants.ResponseMessages.ExternalProvidersAreDisabled;
                logLevel = LogLevel.Debug;
                return false;
            }

            errorCode = SearchResultCode.None;
            errorMessage = string.Empty;
            return true;
        }
    }
}
