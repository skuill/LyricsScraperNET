using LyricsScraperNET.Extensions;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace LyricsScraperNET.Providers
{
    internal sealed class ProviderService : IProviderService
    {
        // Copy-on-write: searches read an immutable snapshot, so one client can be used from many tasks at once
        // while providers are added or removed.
        private volatile IExternalProvider[] _providers = [];
        private readonly object _syncLock = new();

        public IEnumerable<IExternalProvider> GetAvailableProviders(SearchRequest searchRequest)
        {
            var providers = _providers;
            var providerType = searchRequest.GetProviderType();
            return providerType.IsNoneProviderType()
                ? providers.Where(p => p.IsEnabled).OrderByDescending(p => p.SearchPriority).ToList()
                : providers.Where(p => p.IsEnabled && p.Options.ExternalProviderType == providerType).OrderByDescending(p => p.SearchPriority).ToList();
        }

        public IExternalProvider? this[ExternalProviderType providerType]
        {
            // One snapshot: the check and the lookup must see the same providers.
            get => providerType.IsNoneProviderType()
                ? default
                : _providers.FirstOrDefault(p => p.Options.ExternalProviderType == providerType);
        }

        public void AddProvider(IExternalProvider provider)
        {
            lock (_syncLock)
            {
                if (!_providers.Contains(provider))
                {
                    _providers = [.. _providers, provider];
                }
            }
        }

        public void RemoveProvider(ExternalProviderType providerType)
        {
            lock (_syncLock)
            {
                _providers = _providers.Where(p => p.Options.ExternalProviderType != providerType).ToArray();
            }
        }

        public void EnableAllProviders()
        {
            foreach (var provider in _providers)
                provider.Enable();
        }

        public void DisableAllProviders()
        {
            foreach (var provider in _providers)
                provider.Disable();
        }

        public bool AnyEnabled() => _providers.Any(x => x.IsEnabled);
        public bool AnyAvailable() => _providers.Length > 0;

        public void WithLogger(ILoggerFactory loggerFactory)
        {
            foreach (var provider in _providers)
                provider.WithLogger(loggerFactory);
        }

        public void WithHttpClient(HttpClient httpClient)
        {
            foreach (var provider in _providers.OfType<ExternalProviderBase>())
                provider.WithHttpClient(httpClient);
        }

        public bool IsProviderAvailable(ExternalProviderType providerType)
            => !providerType.IsNoneProviderType()
                && AnyAvailable()
                && _providers.Any(p => p.Options.ExternalProviderType == providerType);

        public bool IsProviderEnabled(ExternalProviderType providerType)
            => IsProviderAvailable(providerType)
                && (this[providerType]?.IsEnabled ?? false);
    }
}
