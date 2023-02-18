using LyricsScraperNET.Configuration;
using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Abstract;
using Microsoft.Extensions.Logging;

namespace LyricsScraperNET
{
    public sealed class LyricsScraperClient: ILyricsScraperClient
    {

        private readonly ILogger<LyricsScraperClient> _logger;

        private IList<IExternalProvider> _externalProviders;
        private ILyricScraperClientConfig _lyricScraperClientConfig;

        public bool IsEnabled => _externalProviders != null && _externalProviders.Any(x => x.IsEnabled);

        public LyricsScraperClient() { }

        public LyricsScraperClient(ILogger<LyricsScraperClient> logger,
            ILyricScraperClientConfig lyricScraperClientConfig,
            IEnumerable<IExternalProvider> externalProviders)
        {
            Ensure.ArgumentNotNull(lyricScraperClientConfig, nameof(lyricScraperClientConfig));
            _lyricScraperClientConfig = lyricScraperClientConfig;

            Ensure.ArgumentNotNullOrEmptyList(externalProviders, nameof(externalProviders));
            _externalProviders = externalProviders.ToList();

            _logger = logger;
        }

        public SearchResult SearchLyric(SearchRequest searchRequest)
        {
            if (!ValidateRequest())
                return new SearchResult();

            foreach (var externalProvider in _externalProviders.OrderByDescending(x => x.SearchPriority))
            {
                var searchResult = externalProvider.SearchLyric(searchRequest);
                if (!searchResult.IsEmpty())
                {
                    return searchResult;
                }
                _logger?.LogWarning($"Can't find lyric by provider: {externalProvider}.");
            }
            _logger?.LogError($"Can't find lyrics for searchRequest: {searchRequest}.");
            return new SearchResult();
        }

        public async Task<SearchResult> SearchLyricAsync(SearchRequest searchRequest)
        {
            if (!ValidateRequest())
                return new SearchResult();

            foreach (var externalProvider in _externalProviders.OrderByDescending(x => x.SearchPriority))
            {
                var searchResult = await externalProvider.SearchLyricAsync(searchRequest);
                if (!searchResult.IsEmpty())
                {
                    return searchResult;
                }
                _logger?.LogWarning($"Can't find lyric by provider: {externalProvider}.");
            }
            _logger?.LogError($"Can't find lyrics for searchRequest: {searchRequest}.");
            return new SearchResult();
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

        public void AddProvider(IExternalProvider provider)
        {
            if (IsEmptyProviders())
                _externalProviders = new List<IExternalProvider>();
            if (!_externalProviders.Contains(provider))
                _externalProviders.Add(provider);
            else
                _logger?.LogWarning($"External provider {provider} already added");
        }

        private bool IsEmptyProviders() => _externalProviders == null || !_externalProviders.Any();
    }
}