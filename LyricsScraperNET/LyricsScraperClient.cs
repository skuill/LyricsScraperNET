using LyricsScraperNET.Common;
using LyricsScraperNET.Configuration;
using LyricsScraperNET.Extensions;
using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LyricsScraperNET
{
    public sealed class LyricsScraperClient : ILyricsScraperClient
    {
        private ILoggerFactory _loggerFactory;
        private ILogger<LyricsScraperClient> _logger;

        private List<IExternalProvider> _externalProviders;
        private readonly ILyricScraperClientConfig _lyricScraperClientConfig;

        public bool IsEnabled => _externalProviders != null && _externalProviders.Any(x => x.IsEnabled);

        public IExternalProvider this[ExternalProviderType providerType]
        {
            get => IsProviderAvailable(providerType)
                ? _externalProviders.First(p => p.Options.ExternalProviderType == providerType)
                : null;
        }

        public LyricsScraperClient() { }

        public LyricsScraperClient(ILyricScraperClientConfig lyricScraperClientConfig,
            IEnumerable<IExternalProvider> externalProviders)
        {
            Ensure.ArgumentNotNull(lyricScraperClientConfig, nameof(lyricScraperClientConfig));
            _lyricScraperClientConfig = lyricScraperClientConfig;

            Ensure.ArgumentNotNullOrEmptyList(externalProviders, nameof(externalProviders));
            _externalProviders = externalProviders.ToList();
        }

        public LyricsScraperClient(ILogger<LyricsScraperClient> logger,
            ILyricScraperClientConfig lyricScraperClientConfig,
            IEnumerable<IExternalProvider> externalProviders)
            : this(lyricScraperClientConfig, externalProviders)
        {
            _logger = logger;
        }

        public SearchResult SearchLyric(SearchRequest searchRequest)
        {
            if (!ValidSearchRequestAndConfig(searchRequest, out var searchResult))
            {
                return searchResult;
            }

            foreach (var externalProvider in GetAvailableProvidersForSearchRequest(searchRequest))
            {
                var providerSearchResult = externalProvider.SearchLyric(searchRequest);
                if (!providerSearchResult.IsEmpty() || providerSearchResult.Instrumental)
                {
                    return providerSearchResult;
                }
                _logger?.LogWarning($"Can't find lyric by provider: [{externalProvider.Options?.ExternalProviderType}].");
            }

            searchResult.AddNoDataFoundMessage(Constants.ResponseMessages.NotFoundLyric);
            _logger?.LogError($"Can't find lyrics for searchRequest: [{searchRequest}].");

            return searchResult;
        }

        public async Task<SearchResult> SearchLyricAsync(SearchRequest searchRequest)
        {
            if (!ValidSearchRequestAndConfig(searchRequest, out var searchResult))
            {
                return searchResult;
            }

            foreach (var externalProvider in GetAvailableProvidersForSearchRequest(searchRequest))
            {
                var providerSearchResult = await externalProvider.SearchLyricAsync(searchRequest);
                if (!providerSearchResult.IsEmpty() || providerSearchResult.Instrumental)
                {
                    return providerSearchResult;
                }
                _logger?.LogWarning($"Can't find lyric by provider: [{externalProvider.Options?.ExternalProviderType}].");
            }

            searchResult.AddNoDataFoundMessage(Constants.ResponseMessages.NotFoundLyric);
            _logger?.LogError($"Can't find lyrics for searchRequest: [{searchRequest}].");

            return searchResult;
        }

        private IEnumerable<IExternalProvider> GetAvailableProvidersForSearchRequest(SearchRequest searchRequest)
        {
            var searchRequestExternalProvider = searchRequest.GetProviderTypeFromRequest();

            if (searchRequestExternalProvider.IsNoneProviderType())
                return _externalProviders.Where(p => p.IsEnabled).OrderByDescending(p => p.SearchPriority);

            var availableProviders = _externalProviders.Where(p => p.IsEnabled && p.Options.ExternalProviderType == searchRequestExternalProvider);

            if (availableProviders.Any())
                return availableProviders.OrderByDescending(p => p.SearchPriority);

            return Array.Empty<IExternalProvider>();
        }

        private bool ValidSearchRequestAndConfig(SearchRequest searchRequest, out SearchResult searchResult)
        {
            searchResult = new SearchResult();

            if (!ValidSearchRequest(searchRequest, out var badRequestErrorMessage))
            {
                searchResult.AddBadRequestMessage(badRequestErrorMessage);
                return false;
            }

            if (!ValidClientConfiguration(out var errorMessage))
            {
                searchResult.AddNoDataFoundMessage(errorMessage);
                return false;
            }

            return true;
        }

        private bool ValidClientConfiguration(out string errorMessage)
        {
            errorMessage = string.Empty;
            LogLevel logLevel = LogLevel.Error;

            if (IsEmptyProvidersList())
            {
                errorMessage = Constants.ResponseMessages.ExternalProvidersListIsEmpty;
            }
            else if (!IsEnabled)
            {
                errorMessage = Constants.ResponseMessages.ExternalProvidersAreDisabled;
                logLevel = LogLevel.Debug;
            }

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                _logger?.Log(logLevel, errorMessage);
                return false;
            }
            return true;
        }

        private bool ValidSearchRequest(SearchRequest searchRequest, out string errorMessage)
        {
            LogLevel logLevel = LogLevel.Error;

            if (searchRequest == null)
            {
                errorMessage = Constants.ResponseMessages.SearchRequestIsEmpty;
                _logger?.Log(logLevel, errorMessage);
                return false;
            }

            var isSearchRequestValid = searchRequest.IsValid(out errorMessage);
            if (!isSearchRequestValid)
            {
                _logger?.Log(logLevel, errorMessage);
                return false;
            }

            var searchRequestExternalProvider = searchRequest.GetProviderTypeFromRequest();
            if (!searchRequestExternalProvider.IsNoneProviderType() && !IsProviderEnabled(searchRequestExternalProvider))
            {
                errorMessage = Constants.ResponseMessages.ExternalProviderForRequestNotSpecified;
            }
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                _logger?.Log(logLevel, errorMessage);
                return false;
            }

            return true;
        }

        public void AddProvider(IExternalProvider provider)
        {
            if (IsEmptyProvidersList())
                _externalProviders = new List<IExternalProvider>();
            if (!_externalProviders.Contains(provider))
            {
                if (_loggerFactory != null)
                    provider.WithLogger(_loggerFactory);
                _externalProviders.Add(provider);
            }
            else
                _logger?.LogWarning($"External provider {provider} already added");
        }

        public void RemoveProvider(ExternalProviderType providerType)
        {
            if (providerType.IsNoneProviderType() || IsEmptyProvidersList())
                return;

            _externalProviders.RemoveAll(x => x.Options.ExternalProviderType == providerType);
        }

        public void Enable()
        {
            if (IsEmptyProvidersList())
                return;

            foreach (var provider in _externalProviders)
                provider.Enable();
        }

        public void Disable()
        {
            if (IsEmptyProvidersList())
                return;

            foreach (var provider in _externalProviders)
                provider.Disable();
        }

        public void WithLogger(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<LyricsScraperClient>();

            if (IsEmptyProvidersList())
                return;

            foreach (var provider in _externalProviders)
                provider.WithLogger(loggerFactory);
        }

        private bool IsEmptyProvidersList() => _externalProviders == null || !_externalProviders.Any();

        private bool IsProviderAvailable(ExternalProviderType providerType)
            => !providerType.IsNoneProviderType()
                && !IsEmptyProvidersList()
                && _externalProviders.Any(p => p.Options.ExternalProviderType == providerType);

        private bool IsProviderEnabled(ExternalProviderType providerType)
            => !providerType.IsNoneProviderType()
                && IsProviderAvailable(providerType)
                && this[providerType].IsEnabled;
    }
}