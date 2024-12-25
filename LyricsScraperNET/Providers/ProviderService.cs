using LyricsScraperNET.Extensions;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace LyricsScraperNET.Providers
{
    public class ProviderService : IProviderService
    {
        private List<IExternalProvider> _providers = new();

        public IEnumerable<IExternalProvider> GetAvailableProviders(SearchRequest searchRequest)
        {
            var providerType = searchRequest.GetProviderType();
            return providerType.IsNoneProviderType()
                ? _providers.Where(p => p.IsEnabled).OrderByDescending(p => p.SearchPriority)
                : _providers.Where(p => p.IsEnabled && p.Options.ExternalProviderType == providerType).OrderByDescending(p => p.SearchPriority);
        }

        public IExternalProvider? this[ExternalProviderType providerType]
        {
            get => IsProviderAvailable(providerType)
                ? _providers.FirstOrDefault(p => p.Options.ExternalProviderType == providerType)
                : default;
        }

        public void AddProvider(IExternalProvider provider)
        {
            if (!_providers.Contains(provider))
            {
                _providers.Add(provider);
            }
        }

        public void RemoveProvider(ExternalProviderType providerType)
        {
            _providers.RemoveAll(p => p.Options.ExternalProviderType == providerType);
        }

        public void EnableAllProviders() => _providers.ForEach(p => p.Enable());
        public void DisableAllProviders() => _providers.ForEach(p => p.Disable());

        public bool AnyEnabled() => _providers.Any(x => x.IsEnabled);
        public bool AnyAvailable() => _providers.Any();

        public void WithLogger(ILoggerFactory loggerFactory) => _providers.ForEach(p => p.WithLogger(loggerFactory));

        public bool IsProviderAvailable(ExternalProviderType providerType)
            => !providerType.IsNoneProviderType()
                && AnyAvailable()
                && _providers.Any(p => p.Options.ExternalProviderType == providerType);

        public bool IsProviderEnabled(ExternalProviderType providerType)
            => IsProviderAvailable(providerType)
                && (this[providerType]?.IsEnabled ?? false);
    }
}
