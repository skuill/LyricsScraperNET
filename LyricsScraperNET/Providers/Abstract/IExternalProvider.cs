using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Common;
using Microsoft.Extensions.Logging;
using System;
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

        /// <summary>
        /// Obsolete: blocking wrapper over <see cref="SearchLyricAsync"/>. Removed in 4.0.0.
        /// </summary>
        [Obsolete(Obsoletions.SyncApi.SearchLyricMessage, DiagnosticId = Obsoletions.SyncApi.DiagnosticId, UrlFormat = Obsoletions.SharedUrlFormat)]
        SearchResult SearchLyric(SearchRequest searchRequest, CancellationToken cancellationToken = default);

        Task<SearchResult> SearchLyricAsync(SearchRequest searchRequest, CancellationToken cancellationToken = default);

        void WithParser(IExternalProviderLyricParser parser);

        void WithWebClient(IWebClient webClient);

        void Enable();

        void Disable();

        void WithLogger(ILoggerFactory loggerFactory);
    }
}
