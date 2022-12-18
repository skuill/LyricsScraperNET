using LyricsScraper.Network.Abstract;

namespace LyricsScraper.Abstract
{
    public abstract class LyricClientBase: ILyricClient<string>
    {
        protected ILyricParser<string> Parser { get; set; }
        protected ILyricWebClient WebClient { get; set; }

        public LyricClientBase()
        {
        }

        public abstract string SearchLyric(Uri uri);

        public abstract string SearchLyric(string artist, string song);

        public abstract Task<string> SearchLyricAsync(Uri uri);

        public abstract Task<string> SearchLyricAsync(string artist, string song);

        public void WithParser(ILyricParser<string> parser)
        {
            if (parser != null)
                Parser = parser;
        }

        public void WithWebClient(ILyricWebClient webClient)
        {
            if (webClient != null)
                WebClient = webClient;
        }
    }
}
