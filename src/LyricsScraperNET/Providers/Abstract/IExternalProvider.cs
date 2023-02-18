using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Network.Abstract;

namespace LyricsScraperNET.Providers.Abstract
{
    public interface IExternalProvider<OutputType>
    {
        IExternalProviderOptions Options { get; }

        bool IsEnabled { get; }

        OutputType SearchLyric(SearchRequest searchRequest);

        Task<OutputType> SearchLyricAsync(SearchRequest searchRequest);

        void WithParser(IExternalProviderLyricParser<OutputType> parser);

        void WithWebClient(IWebClient webClient);
    }
}
