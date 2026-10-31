using LyricsScraperNET.Common;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network;
using LyricsScraperNET.Network.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.Abstract
{
    public abstract class ExternalProviderBase : IExternalProvider
    {
        internal IExternalProviderLyricParser Parser { get; set; }
        internal IWebClient WebClient { get; set; }

        public virtual IExternalProviderOptions Options => throw new NotImplementedException();

        public virtual bool IsEnabled => Options != null && Options.Enabled;

        /// <inheritdoc />
        public int SearchPriority => Options != null ? Options.SearchPriority : 0;

        #region Sync (obsolete, removed in 4.0.0)

        /// <inheritdoc />
        [Obsolete(Constants.ObsoleteSyncApi.SearchLyricMessage, DiagnosticId = Constants.ObsoleteSyncApi.DiagnosticId, UrlFormat = Constants.ObsoleteSyncApi.UrlFormat)]
        public virtual SearchResult SearchLyric(SearchRequest searchRequest, CancellationToken cancellationToken = default)
        {
            if (!IsEnabled)
                return new SearchResult();

#pragma warning disable LSNET001 // Calls the obsolete overloads below, so providers that still override them keep working.
            return searchRequest switch
            {
                ArtistAndSongSearchRequest artistAndSongSearchRequest
                    => SearchLyric(artistAndSongSearchRequest.Artist, artistAndSongSearchRequest.Song, cancellationToken),
                UriSearchRequest uriSearchRequest
                    => SearchLyric(uriSearchRequest.Uri, cancellationToken),
                _ => new SearchResult(),
            };
#pragma warning restore LSNET001
        }

        /// <summary>
        /// Blocks on <see cref="SearchLyricAsync(Uri, CancellationToken)"/>. Providers implement only the async overloads.
        /// </summary>
        [Obsolete(Constants.ObsoleteSyncApi.SearchLyricMessage, DiagnosticId = Constants.ObsoleteSyncApi.DiagnosticId, UrlFormat = Constants.ObsoleteSyncApi.UrlFormat)]
        protected virtual SearchResult SearchLyric(Uri uri, CancellationToken cancellationToken = default)
            => SearchLyricAsync(uri, cancellationToken).GetAwaiter().GetResult();

        /// <summary>
        /// Blocks on <see cref="SearchLyricAsync(string, string, CancellationToken)"/>. Providers implement only the async overloads.
        /// </summary>
        [Obsolete(Constants.ObsoleteSyncApi.SearchLyricMessage, DiagnosticId = Constants.ObsoleteSyncApi.DiagnosticId, UrlFormat = Constants.ObsoleteSyncApi.UrlFormat)]
        protected virtual SearchResult SearchLyric(string artist, string song, CancellationToken cancellationToken = default)
            => SearchLyricAsync(artist, song, cancellationToken).GetAwaiter().GetResult();

        #endregion

        #region Async

        public virtual async Task<SearchResult> SearchLyricAsync(SearchRequest searchRequest, CancellationToken cancellationToken = default)
        {
            if (!IsEnabled)
                return new SearchResult();

            return searchRequest switch
            {
                ArtistAndSongSearchRequest artistAndSongSearchRequest
                    => await SearchLyricAsync(artistAndSongSearchRequest.Artist, artistAndSongSearchRequest.Song, cancellationToken).ConfigureAwait(false),
                UriSearchRequest uriSearchRequest
                    => await SearchLyricAsync(uriSearchRequest.Uri, cancellationToken).ConfigureAwait(false),
                _ => new SearchResult(),
            };
        }

        protected virtual Task<SearchResult> SearchLyricAsync(Uri uri, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        protected virtual Task<SearchResult> SearchLyricAsync(string artist, string song, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        #endregion

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
    }
}
