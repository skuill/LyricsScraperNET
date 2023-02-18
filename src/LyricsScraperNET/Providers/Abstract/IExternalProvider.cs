using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network.Abstract;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.Abstract
{
    public interface IExternalProvider
    {
        IExternalProviderOptions Options { get; }

        bool IsEnabled { get; }

        /// <summary>
        /// If there are multiple external providers, then the search will start from the provider with the highest priority.
        /// </summary>
        int SearchPriority { get; }

        SearchResult SearchLyric(SearchRequest searchRequest);

        Task<SearchResult> SearchLyricAsync(SearchRequest searchRequest);

        void WithParser(IExternalProviderLyricParser parser);

        void WithWebClient(IWebClient webClient);
    }
}
