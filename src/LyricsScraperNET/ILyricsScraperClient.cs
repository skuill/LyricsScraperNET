using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Abstract;
using System.Threading.Tasks;

namespace LyricsScraperNET
{
    public interface ILyricsScraperClient
    {
        bool IsEnabled { get; }

        SearchResult SearchLyric(SearchRequest searchRequest);

        Task<SearchResult> SearchLyricAsync(SearchRequest searchRequest);

        void AddProvider(IExternalProvider provider);
    }
}
