using LyricsScraperNET.Common;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Network.Models;
using LyricsScraperNET.Network;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace LyricsScraperNET.Providers.Abstract
{
    public abstract class ExternalProviderBase : IExternalProvider
    {
        internal IExternalProviderLyricParser Parser { get; set; }
        internal IWebClient WebClient { get; set; }

        /// <summary>
        /// The last page load of the current provider call. One provider instance serves concurrent searches, so the state is kept
        /// per call (<see cref="AsyncLocal{T}"/>): set by the public search methods, filled by <see cref="LoadPageAsync"/>.
        /// </summary>
        private static readonly AsyncLocal<LoadTracker?> CurrentLoad = new();

        public virtual IExternalProviderOptions Options => throw new NotImplementedException();

        public virtual bool IsEnabled => Options != null && Options.Enabled;

        /// <inheritdoc />
        public int SearchPriority => Options != null ? Options.SearchPriority : 0;

        #region Sync (obsolete, removed in 4.0.0)

        /// <inheritdoc />
        [Obsolete(Obsoletions.SyncApi.SearchLyricMessage, DiagnosticId = Obsoletions.SyncApi.DiagnosticId, UrlFormat = Obsoletions.SharedUrlFormat)]
        public virtual SearchResult SearchLyric(SearchRequest searchRequest, CancellationToken cancellationToken = default)
        {
            if (!IsEnabled)
                return new SearchResult();

            // A sync method shares the caller's context, so the previous tracker is restored.
            var previousLoad = CurrentLoad.Value;
            var load = new LoadTracker();
            CurrentLoad.Value = load;
            try
            {
#pragma warning disable LSNET0001 // Calls the obsolete overloads below, so providers that still override them keep working.
                var searchResult = searchRequest switch
                {
                    ArtistAndSongSearchRequest artistAndSongSearchRequest
                        => SearchLyric(artistAndSongSearchRequest.Artist, artistAndSongSearchRequest.Song, cancellationToken),
                    UriSearchRequest uriSearchRequest
                        => SearchLyric(uriSearchRequest.Uri, cancellationToken),
                    _ => new SearchResult(),
                };
#pragma warning restore LSNET0001
                return load.ApplyTo(searchResult);
            }
            finally
            {
                CurrentLoad.Value = previousLoad;
            }
        }

        /// <summary>
        /// Blocks on <see cref="SearchLyricAsync(Uri, CancellationToken)"/>. Providers implement only the async overloads.
        /// </summary>
        [Obsolete(Obsoletions.SyncApi.SearchLyricMessage, DiagnosticId = Obsoletions.SyncApi.DiagnosticId, UrlFormat = Obsoletions.SharedUrlFormat)]
        protected virtual SearchResult SearchLyric(Uri uri, CancellationToken cancellationToken = default)
            => SearchLyricAsync(uri, cancellationToken).GetAwaiter().GetResult();

        /// <summary>
        /// Blocks on <see cref="SearchLyricAsync(string, string, CancellationToken)"/>. Providers implement only the async overloads.
        /// </summary>
        [Obsolete(Obsoletions.SyncApi.SearchLyricMessage, DiagnosticId = Obsoletions.SyncApi.DiagnosticId, UrlFormat = Obsoletions.SharedUrlFormat)]
        protected virtual SearchResult SearchLyric(string artist, string song, CancellationToken cancellationToken = default)
            => SearchLyricAsync(artist, song, cancellationToken).GetAwaiter().GetResult();

        #endregion

        #region Async

        public virtual async Task<SearchResult> SearchLyricAsync(SearchRequest searchRequest, CancellationToken cancellationToken = default)
        {
            if (!IsEnabled)
                return new SearchResult();

            // Set inside an async method: the value does not flow back to the caller.
            var load = new LoadTracker();
            CurrentLoad.Value = load;

            var searchResult = searchRequest switch
            {
                ArtistAndSongSearchRequest artistAndSongSearchRequest
                    => await SearchLyricAsync(artistAndSongSearchRequest.Artist, artistAndSongSearchRequest.Song, cancellationToken).ConfigureAwait(false),
                UriSearchRequest uriSearchRequest
                    => await SearchLyricAsync(uriSearchRequest.Uri, cancellationToken).ConfigureAwait(false),
                _ => new SearchResult(),
            };
            return load.ApplyTo(searchResult);
        }

        protected virtual Task<SearchResult> SearchLyricAsync(Uri uri, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        protected virtual Task<SearchResult> SearchLyricAsync(string artist, string song, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        #endregion

        /// <summary>
        /// Loads a page through <see cref="WebClient"/> and remembers the outcome for the current search call,
        /// so the client can tell "the site has no lyrics" from "the site could not be loaded". Providers use it instead of <c>WebClient.LoadAsync</c>.
        /// </summary>
        private protected async Task<string> LoadPageAsync(Uri uri, CancellationToken cancellationToken)
        {
            if (WebClient is not IWebClientWithStatus webClientWithStatus)
                return await WebClient.LoadAsync(uri, cancellationToken).ConfigureAwait(false);

            var loadResult = await webClientWithStatus.LoadWithStatusAsync(uri, cancellationToken).ConfigureAwait(false);
            CurrentLoad.Value?.Record(loadResult);
            return loadResult.Body;
        }

        public void WithParser(IExternalProviderLyricParser parser)
        {
            if (parser != null)
                Parser = parser;
        }

        public void WithWebClient(IWebClient webClient)
        {
            if (webClient != null)
                WebClient = webClient;
        }

        /// <summary>
        /// Sends the requests of this provider through <paramref name="httpClient"/>, for example to set a proxy,
        /// message handlers, resilience policies or an own User-Agent. Replaces the current web client.
        /// The caller owns the client: it is not disposed by the provider and must stay alive while the provider is used.
        /// Reuse one client (or <c>IHttpClientFactory</c>) instead of creating one per provider or request, see
        /// https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient-guidelines.
        /// </summary>
        public void WithHttpClient(HttpClient httpClient)
        {
            WebClient = CreateWebClient(httpClient);
        }

        /// <summary>
        /// The web client used by <see cref="WithHttpClient(HttpClient)"/>. Providers with their own HTTP handling override it.
        /// The web client throws <see cref="ArgumentNullException"/> for a null <paramref name="httpClient"/>.
        /// </summary>
        private protected virtual IWebClient CreateWebClient(HttpClient httpClient)
            => new NetHttpClient(httpClient);

        public void Enable()
        {
            Options?.Enabled = true;
        }

        public void Disable()
        {
            Options?.Enabled = false;
        }

        public virtual void WithLogger(ILoggerFactory loggerFactory)
            => throw new NotImplementedException();

        private sealed class LoadTracker
        {
            private WebLoadResult? _lastLoad;

            internal void Record(WebLoadResult loadResult) => _lastLoad = loadResult;

            /// <summary>
            /// The last load decides: for example Genius falls back from a failed API request to the public search.
            /// </summary>
            internal SearchResult ApplyTo(SearchResult searchResult)
            {
                if (!searchResult.Code.IsSuccess && _lastLoad is { Status: WebLoadStatus.Failed } failedLoad)
                    searchResult.LoadFailure = failedLoad;
                return searchResult;
            }
        }
    }
}
