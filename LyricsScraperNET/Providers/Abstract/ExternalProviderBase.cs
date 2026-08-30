using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network.Abstract;
using Microsoft.Extensions.Logging;
using System;
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

        #region Sync

        public virtual SearchResult SearchLyric(SearchRequest searchRequest, CancellationToken cancellationToken = default)
        {
            if (!IsEnabled)
                return new SearchResult();

            return searchRequest switch
            {
                ArtistAndSongSearchRequest artistAndSongSearchRequest
                    => SearchLyric(artistAndSongSearchRequest.Artist, artistAndSongSearchRequest.Song, cancellationToken),
                UriSearchRequest uriSearchRequest
                    => SearchLyric(uriSearchRequest.Uri, cancellationToken),
                _ => new SearchResult(),
            };
        }

        protected virtual SearchResult SearchLyric(Uri uri, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        protected virtual SearchResult SearchLyric(string artist, string song, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        #endregion

        #region Async

        public virtual async Task<SearchResult> SearchLyricAsync(SearchRequest searchRequest, CancellationToken cancellationToken = default)
        {
            if (!IsEnabled)
                return new SearchResult();

            return searchRequest switch
            {
                ArtistAndSongSearchRequest artistAndSongSearchRequest
                    => await SearchLyricAsync(artistAndSongSearchRequest.Artist, artistAndSongSearchRequest.Song, cancellationToken),
                UriSearchRequest uriSearchRequest
                    => await SearchLyricAsync(uriSearchRequest.Uri, cancellationToken),
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
