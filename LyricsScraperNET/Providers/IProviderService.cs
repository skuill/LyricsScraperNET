using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;

namespace LyricsScraperNET.Providers
{
    internal interface IProviderService
    {
        IEnumerable<IExternalProvider> GetAvailableProviders(SearchRequest searchRequest);
        IExternalProvider? this[ExternalProviderType providerType] { get; }
        bool AnyEnabled();
        bool AnyAvailable();
        bool IsProviderAvailable(ExternalProviderType providerType);
        bool IsProviderEnabled(ExternalProviderType provider);
        void AddProvider(IExternalProvider provider);
        void RemoveProvider(ExternalProviderType providerType);
        void EnableAllProviders();
        void DisableAllProviders();
        void WithLogger(ILoggerFactory loggerFactory);

        void WithHttpClient(HttpClient httpClient);
    }
}
