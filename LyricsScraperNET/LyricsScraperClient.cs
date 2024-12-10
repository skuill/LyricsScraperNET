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
using System.Threading;
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

        public SearchResult SearchLyric(SearchRequest searchRequest, CancellationToken cancellationToken = default)
        {
            try
            {
                // Run async operation synchronously
                return SearchLyricInternal(searchRequest,
                    (provider, ct) => Task.FromResult(provider.SearchLyric(searchRequest, ct)),
                    cancellationToken).Result;
            }
            catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
            {
                // Catch AggregateException and throw OperationCanceledException
                throw ex.InnerException;
            }
        }

        public Task<SearchResult> SearchLyricAsync(SearchRequest searchRequest, CancellationToken cancellationToken = default)
            => SearchLyricInternal(searchRequest,
                (provider, ct) => provider.SearchLyricAsync(searchRequest, ct),
                cancellationToken);

        private async Task<SearchResult> SearchLyricInternal(
            SearchRequest searchRequest,
            Func<IExternalProvider, CancellationToken, Task<SearchResult>> searchAction,
            CancellationToken cancellationToken)
        {
            if (!ValidSearchRequestAndConfig(searchRequest, out var searchResult))
                return searchResult;

            // Create a linked cancellation token to propagate cancellation
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            foreach (var provider in GetAvailableProvidersForSearchRequest(searchRequest))
            {
                // Check for cancellation before each external provider call
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Await the asynchronous search method with the linked cancellation token
                    var result = await searchAction(provider, linkedCts.Token);
                    if (!result.IsEmpty() || result.Instrumental)
                        return result; // Return the result if it is valid
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // Log the cancellation and rethrow the exception
                    _logger?.LogInformation("Search operation was canceled.");
                    throw;
                }
                catch (Exception ex)
                {
                    // Log any unexpected errors to prevent the method from crashing
                    _logger?.LogError(ex, "Error during provider search.");
                }
            }

            // No providers found valid results, log the failure and add a message
            searchResult.AddNoDataFoundMessage(Constants.ResponseMessages.NotFoundLyric);
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