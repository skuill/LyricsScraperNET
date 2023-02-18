using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Network.Abstract;

namespace LyricsScraperNET.Providers.Abstract
{
    public class ExternalProviderBase: IExternalProvider<string>
    {
        internal IExternalProviderLyricParser<string> Parser { get; set; }
        internal IWebClient WebClient { get; set; }

        public virtual IExternalProviderOptions Options => throw new NotImplementedException();

        public virtual bool IsEnabled => Options != null && Options.Enabled;


        #region Sync

        public virtual string SearchLyric(SearchRequest searchRequest)
        {
            if (!IsEnabled)
                return string.Empty;

            switch (searchRequest)
            {
                case ArtistAndSongSearchRequest artistAndSongSearchRequest:
                    return SearchLyric(artistAndSongSearchRequest.Artist, artistAndSongSearchRequest.Song);
                case UriSearchRequest uriSearchRequest:
                    return SearchLyric(uriSearchRequest.Uri);
                default:
                    return string.Empty;
            }
        }

        protected virtual string SearchLyric(Uri uri)
        {
            throw new NotImplementedException();
        }

        protected virtual string SearchLyric(string artist, string song)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region Async

        public virtual async Task<string> SearchLyricAsync(SearchRequest searchRequest)
        {
            if (!IsEnabled)
                return string.Empty;

            switch (searchRequest)
            {
                case ArtistAndSongSearchRequest artistAndSongSearchRequest:
                    return await SearchLyricAsync(artistAndSongSearchRequest.Artist, artistAndSongSearchRequest.Song);
                case UriSearchRequest uriSearchRequest:
                    return await SearchLyricAsync(uriSearchRequest.Uri);
                default:
                    return string.Empty;
            }
        }

        protected virtual Task<string> SearchLyricAsync(Uri uri)
        {
            throw new NotImplementedException();
        }

        protected virtual Task<string> SearchLyricAsync(string artist, string song)
        {
            throw new NotImplementedException();
        }

        #endregion


        public void WithParser(IExternalProviderLyricParser<string> parser)
        {
            if (parser != null)
                Parser = parser;
        }

        public void WithWebClient(IWebClient webClient)
        {
            if (webClient != null)
                WebClient = webClient;
        }
    }
}
