using LyricsScraperNET.Configuration;
using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Providers.Abstract;
using Microsoft.Extensions.Logging;

namespace LyricsScraperNET
{
    public sealed class LyricsScraperClient: ILyricsScraperClient<string>
    {

        private readonly ILogger<LyricsScraperClient> _logger;

        private IList<IExternalProvider<string>> _externalProviders;
        private ILyricScraperClientConfig _lyricScraperClientConfig;

        public bool IsEnabled => _externalProviders != null && _externalProviders.Any(x => x.IsEnabled);

        public LyricsScraperClient() { }

        public LyricsScraperClient(ILogger<LyricsScraperClient> logger,
            ILyricScraperClientConfig lyricScraperClientConfig,
            IEnumerable<IExternalProvider<string>> externalProviders)
        {
            Ensure.ArgumentNotNull(lyricScraperClientConfig, nameof(lyricScraperClientConfig));
            _lyricScraperClientConfig = lyricScraperClientConfig;

            Ensure.ArgumentNotNullOrEmptyList(externalProviders, nameof(externalProviders));
            _externalProviders = externalProviders.ToList();

            _logger = logger;
        }

        public string SearchLyric(SearchRequest searchRequest)
        {
            if (!ValidateRequest())
                return null;

            foreach (var externalProvider in _externalProviders)
            {
                var lyric = externalProvider.SearchLyric(searchRequest);
                if (!string.IsNullOrEmpty(lyric))
                {
                    return lyric;
                }
                _logger?.LogWarning($"Can't find lyric by provider: {externalProvider}.");
            }
            _logger?.LogError($"Can't find lyrics for searchRequest: {searchRequest}.");
            return null;
        }

        public async Task<string> SearchLyricAsync(SearchRequest searchRequest)
        {
            if (!ValidateRequest())
                return null;

            foreach (var externalProvider in _externalProviders)
            {
                var lyric = await externalProvider.SearchLyricAsync(searchRequest);
                if (!string.IsNullOrEmpty(lyric))
                {
                    return lyric;
                }
                _logger?.LogWarning($"Can't find lyric by provider: {externalProvider}.");
            }
            _logger?.LogError($"Can't find lyrics for searchRequest: {searchRequest}.");
            return null;
        }

        private bool ValidateRequest()
        {
            string error = string.Empty;
            LogLevel logLevel = LogLevel.Error;

            if (IsEmptyProviders())
            {
                error = "Empty providers list! Please set any external provider first.";
            }
            else if (!IsEnabled)
            {
                error = "All external providers is disabled. Searching lyrics is disabled.";
                logLevel = LogLevel.Warning;
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                _logger?.Log(logLevel, error);
                return false;
            }    
            return true;
        }

        public void AddProvider(IExternalProvider<string> provider)
        {
            if (IsEmptyProviders())
                _externalProviders = new List<IExternalProvider<string>>();
            if (!_externalProviders.Contains(provider))
                _externalProviders.Add(provider);
            else
                _logger?.LogWarning($"External provider {provider} already added");
        }

        private bool IsEmptyProviders() => _externalProviders == null || !_externalProviders.Any();
    }
}