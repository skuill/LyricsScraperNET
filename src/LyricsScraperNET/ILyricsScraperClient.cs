using LyricsScraperNET.Abstract;
using LyricsScraperNET.Models;

namespace LyricsScraper
{
    public interface ILyricsScraperClient<T>
    {
        T SearchLyric(SearchRequest searchRequest);

        Task<T> SearchLyricAsync(SearchRequest searchRequest);

        void AddClient(IExternalServiceClient<T> client);
    }
}
