using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network.Abstract;
using Microsoft.Extensions.Logging;
using System.Threading;
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

        SearchResult SearchLyric(SearchRequest searchRequest, CancellationToken cancellationToken);

        Task<SearchResult> SearchLyricAsync(SearchRequest searchRequest, CancellationToken cancellationToken);

        void WithParser(IExternalProviderLyricParser parser);

        void WithWebClient(IWebClient webClient);

        void Enable();

        void Disable();

        void WithLogger(ILoggerFactory loggerFactory);
    }
}
