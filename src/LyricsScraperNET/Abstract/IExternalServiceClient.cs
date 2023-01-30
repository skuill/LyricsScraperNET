using LyricsScraperNET.Models;
using LyricsScraperNET.Network.Abstract;

namespace LyricsScraperNET.Abstract
{
    public interface IExternalServiceClient<T>
    {
        T SearchLyric(SearchRequest searchRequest);

        Task<T> SearchLyricAsync(SearchRequest searchRequest);

        void WithParser(IExternalServiceLyricParser<T> parser);

        void WithWebClient(IWebClient webClient);
    }
}
