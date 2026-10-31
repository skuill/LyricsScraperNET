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
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET
{
    public sealed class LyricsScraperClient : ILyricsScraperClient
    {
        private ILoggerFactory? _loggerFactory;
        private ILogger<LyricsScraperClient>? _logger;

        private ProviderService _providerService;
        private HttpClient? _httpClient;
        private IRequestValidator _requestValidator;
        private readonly ILyricScraperClientConfig? _lyricScraperClientConfig;

        private bool? _useParallelSearch;
        /// <inheritdoc />
        public bool UseParallelSearch
        {
            get => _useParallelSearch ?? _lyricScraperClientConfig?.UseParallelSearch ?? false;
            set => _useParallelSearch = value;
        }

        // Stored as ticks: a TimeSpan? write is not atomic, and the value can be changed while other threads search.
        private const long ProviderTimeoutNotSet = long.MinValue;
        private long _providerTimeoutTicks = ProviderTimeoutNotSet;
        /// <inheritdoc />
        public TimeSpan ProviderTimeout
        {
            get
            {
                var ticks = Volatile.Read(ref _providerTimeoutTicks);
                if (ticks != ProviderTimeoutNotSet)
                    return TimeSpan.FromTicks(ticks);

                var configTimeout = _lyricScraperClientConfig?.ProviderTimeout;
                return configTimeout.HasValue && IsValidTimeout(configTimeout.Value)
                    ? configTimeout.Value
                    : Constants.DefaultProviderTimeout;
            }
            set
            {
                if (!IsValidTimeout(value))
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The timeout must be positive or Timeout.InfiniteTimeSpan.");
                Volatile.Write(ref _providerTimeoutTicks, value.Ticks);
            }
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
        [Obsolete(Obsoletions.SyncApi.SearchLyricMessage, DiagnosticId = Obsoletions.SyncApi.DiagnosticId, UrlFormat = Obsoletions.SharedUrlFormat)]
        public SearchResult SearchLyric(SearchRequest searchRequest, CancellationToken cancellationToken = default)
        {
            // Settings are read once, so a concurrent change cannot mix modes within one search.
            var useParallelSearch = UseParallelSearch;

            // Parallel search always uses the async provider methods, so only the caller thread is blocked
            // instead of one thread per provider. Sequential search calls the sync provider methods on the caller thread
            // (no extra thread pool threads); the timeout then works through the provider's cancellation token.
            var settings = new SearchSettings(useParallelSearch, ProviderTimeout, RunProviderOnCallerThread: !useParallelSearch);
            Func<IExternalProvider, CancellationToken, Task<SearchResult>> searchAction = useParallelSearch
                ? (provider, ct) => provider.SearchLyricAsync(searchRequest, ct)
#pragma warning disable LSNET0001 // The obsolete sync client API keeps using the sync provider API until both are removed in 4.0.0 (#79).
                : (provider, ct) => Task.FromResult(provider.SearchLyric(searchRequest, ct));
#pragma warning restore LSNET0001

            return SearchLyricInternal(searchRequest, searchAction, settings, cancellationToken).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public Task<SearchResult> SearchLyricAsync(SearchRequest searchRequest, CancellationToken cancellationToken = default)
            => SearchLyricInternal(searchRequest,
                (provider, ct) => provider.SearchLyricAsync(searchRequest, ct),
                new SearchSettings(UseParallelSearch, ProviderTimeout, RunProviderOnCallerThread: false),
                cancellationToken);

        /// <summary>
        /// Client settings captured once at the start of a search.
        /// <paramref name="RunProviderOnCallerThread"/>: call the provider directly instead of starting it on the thread pool.
        /// </summary>
        private readonly record struct SearchSettings(bool UseParallelSearch, TimeSpan ProviderTimeout, bool RunProviderOnCallerThread);

        private async Task<SearchResult> SearchLyricInternal(
            SearchRequest searchRequest,
            Func<IExternalProvider, CancellationToken, Task<SearchResult>> searchAction,
            SearchSettings settings,
            CancellationToken cancellationToken = default)
        {
            if (!ValidSearchRequestAndConfig(searchRequest, out var searchResult))
                return searchResult;

            // Filled only by this method's search loop, never by provider tasks.
            var attempts = new List<ProviderAttempt>();
            var foundResult = settings.UseParallelSearch
                ? await SearchInParallel(searchRequest, searchAction, settings, attempts, cancellationToken).ConfigureAwait(false)
                : await SearchSequentially(searchRequest, searchAction, settings, attempts, cancellationToken).ConfigureAwait(false);

            var providerAttempts = attempts.ToArray();
            if (foundResult != null)
                return foundResult.WithProviderAttempts(providerAttempts);

            // NotFound as soon as one provider answered; AllProvidersFailed if none could check (the lyrics may exist).
            var anyAnswer = providerAttempts.Length == 0 || providerAttempts.Any(attempt => attempt.Outcome.IsAnswer);
            searchResult.WithCode(
                anyAnswer ? SearchResultCode.NotFound : SearchResultCode.AllProvidersFailed,
                anyAnswer ? Constants.ResponseMessages.NotFoundLyric : Constants.ResponseMessages.AllProvidersFailed);
            searchResult.ProviderAttempts = providerAttempts;
            return searchResult;
        }

        private async Task<SearchResult?> SearchSequentially(
            SearchRequest searchRequest,
            Func<IExternalProvider, CancellationToken, Task<SearchResult>> searchAction,
            SearchSettings settings,
            List<ProviderAttempt> attempts,
            CancellationToken cancellationToken)
        {
            foreach (var provider in _providerService.GetAvailableProviders(searchRequest))
            {
                // Check for cancellation before each external provider call
                cancellationToken.ThrowIfCancellationRequested();

                var outcome = await SearchWithProvider(provider, searchAction, settings, cancellationToken).ConfigureAwait(false);
                attempts.Add(outcome.Attempt);

                // A provider canceled by the caller returns no result, so the cancellation has to be rethrown here.
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger?.LogInformation("Search operation was canceled.");
                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (outcome.FoundResult != null)
                    return outcome.FoundResult;
            }

            return null;
        }

        /// <summary>
        /// Starts all providers at once. A result is returned as soon as no provider with a higher <see cref="IExternalProvider.SearchPriority"/>
        /// is still running, so the result does not depend on which site answers first. Among providers with the same priority the fastest one wins.
        /// The remaining providers are canceled and recorded as <see cref="ProviderAttemptOutcome.Canceled"/>.
        /// Caller cancellation is rethrown as <see cref="OperationCanceledException"/>.
        /// </summary>
        private async Task<SearchResult?> SearchInParallel(
            SearchRequest searchRequest,
            Func<IExternalProvider, CancellationToken, Task<SearchResult>> searchAction,
            SearchSettings settings,
            List<ProviderAttempt> attempts,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // SearchWithProvider starts the provider on the thread pool, so starting all providers here does not block.
            // Priorities are captured once: a concurrent options change cannot change the decision during the search.
            var running = _providerService.GetAvailableProviders(searchRequest)
                .Select(provider => (
                    Priority: provider.SearchPriority,
                    ProviderType: GetProviderType(provider),
                    StartedAt: Stopwatch.GetTimestamp(),
                    Task: SearchWithProvider(provider, searchAction, settings, linkedCts.Token)))
                .ToList();

            // Found results in completion order.
            var found = new List<(int Priority, SearchResult Result)>();

            try
            {
                while (running.Count > 0)
                {
                    // Each task completes on caller cancellation too (SearchWithProvider stops waiting), so the loop wakes up.
                    var completedTask = await Task.WhenAny(running.Select(x => x.Task)).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();

                    var completed = running.First(x => x.Task == completedTask);
                    running.Remove(completed);

                    var outcome = await completed.Task.ConfigureAwait(false);
                    attempts.Add(outcome.Attempt);
                    if (outcome.FoundResult != null)
                        found.Add((completed.Priority, outcome.FoundResult));

                    if (found.Count == 0)
                        continue;

                    // OrderByDescending is stable: among equal priorities the earliest completed result is taken.
                    var best = found.OrderByDescending(x => x.Priority).First();
                    if (running.Any(x => x.Priority > best.Priority))
                        continue;

                    foreach (var stopped in running)
                    {
                        attempts.Add(new ProviderAttempt(stopped.ProviderType, ProviderAttemptOutcome.Canceled,
                            Stopwatch.GetElapsedTime(stopped.StartedAt), Constants.ResponseMessages.ProviderStoppedResultFound));
                    }

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

        /// <param name="FoundResult">The provider result if it has lyrics or is instrumental, otherwise null.</param>
        /// <param name="Attempt">What the provider did, for <see cref="SearchResult.ProviderAttempts"/>.</param>
        private readonly record struct ProviderSearchOutcome(SearchResult? FoundResult, ProviderAttempt Attempt);

        /// <summary>
        /// Runs one provider. The provider gets <see cref="ProviderTimeout"/> to answer;
        /// provider errors and timeouts are logged, recorded in the attempt and not rethrown.
        /// </summary>
        private async Task<ProviderSearchOutcome> SearchWithProvider(
            IExternalProvider provider,
            Func<IExternalProvider, CancellationToken, Task<SearchResult>> searchAction,
            SearchSettings settings,
            CancellationToken cancellationToken)
        {
            var providerType = GetProviderType(provider);
            var startedAt = Stopwatch.GetTimestamp();
            var timeout = settings.ProviderTimeout;
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (timeout != Timeout.InfiniteTimeSpan)
                timeoutCts.CancelAfter(timeout);

            ProviderSearchOutcome Outcome(ProviderAttemptOutcome outcome, string? message = null, SearchResult? foundResult = null)
                => new(foundResult, new ProviderAttempt(providerType, outcome, Stopwatch.GetElapsedTime(startedAt), message));

            Task<SearchResult>? searchTask = null;
            try
            {
                // Task.Run: a provider that blocks synchronously before returning its task must not delay the timeout below.
                searchTask = settings.RunProviderOnCallerThread
                    ? searchAction(provider, timeoutCts.Token)
                    : Task.Run(() => searchAction(provider, timeoutCts.Token), CancellationToken.None);

                // WaitAsync also stops waiting for a provider that ignores its cancellation token.
                var result = timeout == Timeout.InfiniteTimeSpan
                    ? await searchTask.WaitAsync(cancellationToken).ConfigureAwait(false)
                    : await searchTask.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);

                if (IsFound(result))
                    return Outcome(result!.Instrumental ? ProviderAttemptOutcome.Instrumental : ProviderAttemptOutcome.Found, foundResult: result);

                return result?.LoadFailure is { } loadFailure
                    ? Outcome(ProviderAttemptOutcome.Failed, loadFailure.FailureMessage)
                    : Outcome(ProviderAttemptOutcome.NotFound);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Canceled by the caller or because the search already has a result.
                ObserveAbandonedTask(searchTask);
                return Outcome(ProviderAttemptOutcome.Canceled);
            }
            catch (Exception ex) when (ex is TimeoutException || (ex is OperationCanceledException && timeoutCts.IsCancellationRequested))
            {
                ObserveAbandonedTask(searchTask);
                _logger?.LogWarning("Provider {Provider} did not answer within {Timeout}.", provider.GetType().Name, timeout);
                return Outcome(ProviderAttemptOutcome.Timeout, $"No answer within {timeout}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in provider {Provider}.", provider.GetType().Name);
                return Outcome(ProviderAttemptOutcome.Error, $"{ex.GetType().Name}: {ex.Message}");
            }
            finally
            {
                // WaitAsync can return before the parent cancellation reaches timeoutCts,
                // so cancel it explicitly before it is disposed: a provider that is still running must be stopped.
                timeoutCts.Cancel();
            }
        }

        private static ExternalProviderType GetProviderType(IExternalProvider provider)
            => provider.Options?.ExternalProviderType ?? ExternalProviderType.None;

        /// <summary>
        /// The search stopped waiting for this provider task. If the task fails later, its exception is observed here,
        /// so it does not surface as <see cref="TaskScheduler.UnobservedTaskException"/>.
        /// </summary>
        private static void ObserveAbandonedTask(Task? task)
        {
            if (task == null || task.IsCompleted)
                return;

            task.ContinueWith(
                static t => _ = t.Exception,
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        private static bool IsFound(SearchResult? result)
            => result != null && (!result.IsEmpty() || result.Instrumental);

        private static bool IsValidTimeout(TimeSpan timeout)
            => timeout == Timeout.InfiniteTimeSpan || timeout > TimeSpan.Zero;

        private bool ValidSearchRequestAndConfig(SearchRequest searchRequest, out SearchResult searchResult)
        {
            searchResult = new SearchResult();

            if (!_requestValidator.IsValidSearchRequest(_providerService, searchRequest, out var errorCode, out var errorMessage, out var logLevel)
                || !_requestValidator.IsValidClientConfiguration(_providerService, out errorCode, out errorMessage, out logLevel))
            {
                _logger?.Log(logLevel, errorMessage);
                searchResult.WithCode(errorCode, errorMessage);
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
            if (_httpClient != null && provider is ExternalProviderBase externalProviderBase)
                externalProviderBase.WithHttpClient(_httpClient);
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

        /// <inheritdoc />
        public ILyricsScraperClient WithHttpClient(HttpClient httpClient)
        {
            Ensure.ArgumentNotNull(httpClient, nameof(httpClient));
            _httpClient = httpClient;
            _providerService.WithHttpClient(httpClient);
            return this;
        }
    }
}