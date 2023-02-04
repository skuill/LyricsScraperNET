using LyricsScraperNET.External.Abstract;
using LyricsScraperNET.Models;

namespace LyricsScraper
{
    public interface ILyricsScraperClient<OutputType>
    {
        OutputType SearchLyric(SearchRequest searchRequest);

        Task<OutputType> SearchLyricAsync(SearchRequest searchRequest);

        void AddClient(IExternalServiceClient<OutputType> client);
    }
}
