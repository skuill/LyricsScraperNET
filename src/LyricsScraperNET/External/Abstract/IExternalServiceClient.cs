using LyricsScraperNET.Models;
using LyricsScraperNET.Network.Abstract;

namespace LyricsScraperNET.External.Abstract
{
    public interface IExternalServiceClient<OutputType>
    {
        IExternalServiceClientOptions Options { get; }

        bool IsEnabled { get; }

        OutputType SearchLyric(SearchRequest searchRequest);

        Task<OutputType> SearchLyricAsync(SearchRequest searchRequest);

        void WithParser(IExternalServiceLyricParser<OutputType> parser);

        void WithWebClient(IWebClient webClient);
    }
}
