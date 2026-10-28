using LyricsScraperNET.Common;
using LyricsScraperNET.Configuration;
using LyricsScraperNET.Extensions;
using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.Validations;
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
        private ILoggerFactory? _loggerFactory;
        private ILogger<LyricsScraperClient>? _logger;

        private IProviderService _providerService;
        private IRequestValidator _requestValidator;
        private readonly ILyricScraperClientConfig? _lyricScraperClientConfig;

        private bool? _useParallelSearch;
        /// <inheritdoc />
        public bool UseParallelSearch
        {
            get => _useParallelSearch ?? _lyricScraperClientConfig?.UseParallelSearch ?? false;
            set => _useParallelSearch = value;
        }

        /// <inheritdoc />
        public bool IsEnabled => _providerService.AnyEnabled();

        /// <inheritdoc />
        public IExternalProvider? this[ExternalProviderType providerType]
        {
            get => _providerService[providerType];
        }

        public LyricsScraperClient()
        {
            _providerService = new ProviderService();
            _requestValidator = new RequestValidator();
        }

        public LyricsScraperClient(ILyricScraperClientConfig lyricScraperClientConfig,
            IEnumerable<IExternalProvider> externalProviders)
            : this()
        {
            Ensure.ArgumentNotNull(lyricScraperClientConfig, nameof(lyricScraperClientConfig));
            _lyricScraperClientConfig = lyricScraperClientConfig;

            Ensure.ArgumentNotNullOrEmptyList(externalProviders, nameof(externalProviders));
            foreach (var externalProvider in externalProviders)
            {
                _providerService.AddProvider(externalProvider);
            }
        }

        public LyricsScraperClient(ILogger<LyricsScraperClient> logger,
            ILyricScraperClientConfig lyricScraperClientConfig,
            IEnumerable<IExternalProvider> externalProviders)
            : this(lyricScraperClientConfig, externalProviders)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public SearchResult SearchLyric(SearchRequest searchRequest, CancellationToken cancellationToken = default)
        {
            // Parallel search always uses the async provider methods, so only the caller thread is blocked
            // instead of one thread per provider. Sequential search calls the sync provider methods.
            Func<IExternalProvider, CancellationToken, Task<SearchResult>> searchAction = UseParallelSearch
                ? (provider, ct) => provider.SearchLyricAsync(searchRequest, ct)
                : (provider, ct) => Task.FromResult(provider.SearchLyric(searchRequest, ct));

            return SearchLyricInternal(searchRequest, searchAction, cancellationToken).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public Task<SearchResult> SearchLyricAsync(SearchRequest searchRequest, CancellationToken cancellationToken = default)
            => SearchLyricInternal(searchRequest,
                (provider, ct) => provider.SearchLyricAsync(searchRequest, ct),
                cancellationToken);

        private async Task<SearchResult> SearchLyricInternal(
            SearchRequest searchRequest,
            Func<IExternalProvider, CancellationToken, Task<SearchResult>> searchAction,
            CancellationToken cancellationToken = default)
        {
            if (!ValidSearchRequestAndConfig(searchRequest, out var searchResult))
                return searchResult;

            var foundResult = UseParallelSearch
                ? await SearchInParallel(searchRequest, searchAction, cancellationToken).ConfigureAwait(false)
                : await SearchSequentially(searchRequest, searchAction, cancellationToken).ConfigureAwait(false);

            if (foundResult != null)
                return foundResult;

            // No providers found valid results, log the failure and add a message
            searchResult.AddNoDataFoundMessage(Constants.ResponseMessages.NotFoundLyric);
            return searchResult;
        }

        private async Task<SearchResult?> SearchSequentially(
            SearchRequest searchRequest,
            Func<IExternalProvider, CancellationToken, Task<SearchResult>> searchAction,
            CancellationToken cancellationToken)
        {
            foreach (var provider in _providerService.GetAvailableProviders(searchRequest))
            {
                // Check for cancellation before each external provider call
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var result = await searchAction(provider, cancellationToken).ConfigureAwait(false);
                    if (IsFound(result))
                        return result;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger?.LogInformation("Search operation was canceled.");
                    throw;
                }
                catch (Exception ex)
                {
                    // Log any unexpected errors to prevent the method from crashing
                    _logger?.LogError(ex, "Error in provider {Provider}.", provider.GetType().Name);
                }
            }

            return null;
        }

        /// <summary>
        /// Starts all providers at once. A result is returned as soon as no provider with a higher <see cref="IExternalProvider.SearchPriority"/>
        /// is still running, so the result does not depend on which site answers first. Among providers with the same priority the fastest one wins.
        /// The remaining providers are canceled. Caller cancellation is rethrown as <see cref="OperationCanceledException"/>.
        /// </summary>
        private async Task<SearchResult?> SearchInParallel(
            SearchRequest searchRequest,
            Func<IExternalProvider, CancellationToken, Task<SearchResult>> searchAction,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var running = _providerService.GetAvailableProviders(searchRequest)
                .Select(provider => (Provider: provider, Task: Task.Run(() => SearchWithProvider(provider, searchAction, linkedCts.Token))))
                .ToList();

            // Completes when the caller cancels, even if a provider ignores its cancellation token.
            var cancellationTask = Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);

            // Found results in completion order.
            var found = new List<(int Priority, SearchResult Result)>();

            try
            {
                while (running.Count > 0)
                {
                    var completedTask = await Task.WhenAny(running.Select(x => (Task)x.Task).Append(cancellationTask)).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();

                    var completed = running.First(x => x.Task == completedTask);
                    running.Remove(completed);

                    var result = await completed.Task.ConfigureAwait(false);
                    if (result != null)
                        found.Add((completed.Provider.SearchPriority, result));

                    if (found.Count == 0)
                        continue;

                    // OrderByDescending is stable: among equal priorities the earliest completed result is taken.
                    var best = found.OrderByDescending(x => x.Priority).First();
                    if (!running.Any(x => x.Provider.SearchPriority > best.Priority))
                        return best.Result;
                }

                return null;
            }
            finally
            {
                // Stop the providers that are still running.
                linkedCts.Cancel();
            }
        }

        /// <summary>
        /// Returns the found result or null. Provider errors are logged and not rethrown.
        /// </summary>
        private async Task<SearchResult?> SearchWithProvider(
            IExternalProvider provider,
            Func<IExternalProvider, CancellationToken, Task<SearchResult>> searchAction,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await searchAction(provider, cancellationToken).ConfigureAwait(false);
                return IsFound(result) ? result : null;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Canceled by the caller or because the search already has a result.
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in provider {Provider}.", provider.GetType().Name);
                return null;
            }
        }

        private static bool IsFound(SearchResult? result)
            => result != null && (!result.IsEmpty() || result.Instrumental);

        private bool ValidSearchRequestAndConfig(SearchRequest searchRequest, out SearchResult searchResult)
        {
            searchResult = new SearchResult();
            LogLevel logLevel;
            string errorMessage;

            if (!_requestValidator.IsValidSearchRequest(_providerService, searchRequest, out errorMessage, out logLevel))
            {
                _logger?.Log(logLevel, errorMessage);
                searchResult.AddBadRequestMessage(errorMessage);
                return false;
            }

            if (!_requestValidator.IsValidClientConfiguration(_providerService, out errorMessage, out logLevel))
            {
                _logger?.Log(logLevel, errorMessage);
                searchResult.AddNoDataFoundMessage(errorMessage);
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public void AddProvider(IExternalProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }
            if (_loggerFactory != null)
                provider.WithLogger(_loggerFactory);
            _providerService.AddProvider(provider);
        }

        /// <inheritdoc />
        public void RemoveProvider(ExternalProviderType providerType)
        {
            if (providerType.IsNoneProviderType())
                return;

            _providerService.RemoveProvider(providerType);
        }

        /// <inheritdoc />
        public void Enable() => _providerService.EnableAllProviders();

        /// <inheritdoc />
        public void Disable() => _providerService.DisableAllProviders();

        /// <inheritdoc />
        public void WithLogger(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _providerService.WithLogger(loggerFactory);
        }
    }
}