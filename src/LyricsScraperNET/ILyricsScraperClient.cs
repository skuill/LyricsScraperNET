using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Providers.Abstract;

namespace LyricsScraperNET
{
    public interface ILyricsScraperClient<OutputType>
    {
        bool IsEnabled { get; }

        OutputType SearchLyric(SearchRequest searchRequest);

        Task<OutputType> SearchLyricAsync(SearchRequest searchRequest);

        void AddProvider(IExternalProvider<OutputType> provider);
    }
}
