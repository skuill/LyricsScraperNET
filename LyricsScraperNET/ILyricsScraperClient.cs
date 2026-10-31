using LyricsScraperNET.Common;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET
{
    public interface ILyricsScraperClient
    {
        /// <summary>
        /// Checking that there is some enabled external provider available for search.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Determines whether to use parallel search instead of sequential search.
        /// If explicitly set, the value from the local variable is used.
        /// Otherwise, it falls back to the configuration setting.
        /// By default, sequential search is used if not specified.
        ///
        /// Parallel search starts all providers at once and returns a result as soon as no provider with a higher
        /// search priority is still running; among providers with the same priority the fastest one wins.
        /// The remaining providers are canceled.
        /// </summary>
        bool UseParallelSearch { get; set; }

        /// <summary>
        /// Maximum time one external provider gets to answer. A provider that does not answer in time is treated as "not found"
        /// and the search continues with the other providers.
        /// If explicitly set, the value from the local variable is used. Otherwise, it falls back to the configuration setting.
        /// Default: 10 seconds. Use <see cref="Timeout.InfiniteTimeSpan"/> to disable the timeout.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is not positive and not <see cref="Timeout.InfiniteTimeSpan"/>.</exception>
        TimeSpan ProviderTimeout { get; set; }

        IExternalProvider? this[ExternalProviderType providerType] { get; }

        /// <summary>
        /// Search lyric by different search requests:
        /// 1) Search by Uri: <see cref="UriSearchRequest"/>
        /// 2) Search by Artist and Song name: <see cref="ArtistAndSongSearchRequest"/>
        /// 
        /// Supports cancellation via <see cref="CancellationToken"/>.
        /// Throws <see cref="OperationCanceledException"/> if the operation is canceled.
        ///
        /// Obsolete: blocks the calling thread until the search completes, can deadlock with a <see cref="SynchronizationContext"/>
        /// and can starve the thread pool. Use <see cref="SearchLyricAsync"/>. Removed in 4.0.0.
        /// </summary>
        [Obsolete(Obsoletions.SyncApi.SearchLyricMessage, DiagnosticId = Obsoletions.SyncApi.DiagnosticId, UrlFormat = Obsoletions.SharedUrlFormat)]
        SearchResult SearchLyric(SearchRequest searchRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Async search lyric by different search requests:
        /// 1) Search by Uri: <see cref="UriSearchRequest"/>
        /// 2) Search by Artist and Song name: <see cref="ArtistAndSongSearchRequest"/>
        /// 
        /// Supports cancellation via <see cref="CancellationToken"/>. 
        /// Throws <see cref="OperationCanceledException"/> if the operation is canceled.
        /// </summary>
        Task<SearchResult> SearchLyricAsync(SearchRequest searchRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adding a new external provider that will be used to search for lyrics.
        /// </summary>
        void AddProvider(IExternalProvider provider);

        /// <summary>
        /// Removing an external provider by <paramref name="providerType"/> from the list of search providers.
        /// </summary>
        void RemoveProvider(ExternalProviderType providerType);

        /// <summary>
        /// Enable lyrics search. All external providers will be enabled in this case.
        /// </summary>
        void Enable();

        /// <summary>
        /// Disable lyrics search. All external providers will be enabled in this case.
        /// Calling the lyrics search method will return an empty result.
        /// </summary>
        void Disable();

        /// <summary>
        /// Creates a new ILogger instance from <paramref name="loggerFactory"/>.
        /// Can be useful for error analysis if the lyric text is not found.
        /// </summary>
        void WithLogger(ILoggerFactory loggerFactory);

        /// <summary>
        /// Sends the requests of all providers through <paramref name="httpClient"/>: the providers already added and the ones added later,
        /// for example <c>new LyricsScraperClient().WithHttpClient(httpClient).WithAllProviders()</c>.
        /// Use it to set a proxy, message handlers, resilience policies or an own User-Agent.
        /// Applies to providers based on <see cref="ExternalProviderBase"/>; see <see cref="ExternalProviderBase.WithHttpClient(HttpClient)"/>.
        /// The caller owns the client: it is not disposed by the library and must stay alive while the client is used.
        /// With dependency injection configure the <c>ServiceCollectionExtensions.HttpClientName</c> client instead.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="httpClient"/> is null.</exception>
        ILyricsScraperClient WithHttpClient(HttpClient httpClient);
    }
}
