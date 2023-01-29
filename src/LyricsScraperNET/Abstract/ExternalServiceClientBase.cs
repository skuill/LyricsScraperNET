using LyricsScraperNET.Network.Abstract;

namespace LyricsScraperNET.Abstract
{
    public abstract class ExternalServiceClientBase: IExternalServiceClient<string>
    {
        protected IExternalServiceLyricParser<string> Parser { get; set; }
        protected IWebClient WebClient { get; set; }

        public ExternalServiceClientBase()
        {
        }

        public abstract string SearchLyric(Uri uri);

        public abstract string SearchLyric(string artist, string song);

        public abstract Task<string> SearchLyricAsync(Uri uri);

        public abstract Task<string> SearchLyricAsync(string artist, string song);

        public void WithParser(IExternalServiceLyricParser<string> parser)
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
