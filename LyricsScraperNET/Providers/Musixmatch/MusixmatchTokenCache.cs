using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MusixmatchClientLib.Auth;

namespace LyricsScraperNET.Providers.Musixmatch
{
    public sealed class MusixmatchTokenCache : IMusixmatchTokenCache
    {
        private ILogger<MusixmatchTokenCache>? _logger;

        // Musixmatch Token memory cache
        private static IMemoryCache? _memoryCache;
        private static MemoryCacheEntryOptions? _memoryCacheEntryOptions;

        private static readonly object _syncLock = new object();

        private readonly string MusixmatchTokenKey = "MusixmatchToken";

        public MusixmatchTokenCache()
        {
            InitializeMemoryCache();
            InitializeMemoryCacheEntryOptions();
        }

        private void InitializeMemoryCache()
        {
            if (_memoryCache == null)
            {
                lock (_syncLock)
                {
                    if (_memoryCache == null)
                    {
                        _memoryCache = new MemoryCache(new MemoryCacheOptions()
                        {
                            SizeLimit = 1024,
                        });
                    }
                }
            }
        }

        private void InitializeMemoryCacheEntryOptions()
        {
            if (_memoryCacheEntryOptions == null)
            {
                lock (_syncLock)
                {
                    if (_memoryCacheEntryOptions == null)
                    {
                        _memoryCacheEntryOptions = new MemoryCacheEntryOptions()
                        {
                            Size = 1
                        };
                    }
                }
            }
        }

        public MusixmatchTokenCache(ILogger<MusixmatchTokenCache> logger) : this()
        {
            _logger = logger;
        }

        public string GetOrCreateToken(bool regenerate = false)
        {
            if (regenerate)
                _memoryCache!.Remove(MusixmatchTokenKey);

            _logger?.LogDebug("Musixmatch. Use default MusixmatchToken.");
            string musixmatchTokenValue;
            if (!_memoryCache.TryGetValue(MusixmatchTokenKey, out musixmatchTokenValue))
            {
                _logger?.LogDebug("Musixmatch. Generate new token.");
                var musixmatchToken = new MusixmatchToken();
                musixmatchTokenValue = musixmatchToken.Token;
                _memoryCache.Set(MusixmatchTokenKey, musixmatchTokenValue, _memoryCacheEntryOptions);
            }
            return musixmatchTokenValue;
        }
    }
}
