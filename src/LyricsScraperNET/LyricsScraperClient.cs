using LyricsScraperNET.External.Abstract;
using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models;
using Microsoft.Extensions.Logging;

namespace LyricsScraper
{
    public sealed class LyricsScraperClient: ILyricsScraperClient<string>
    {
        private IList<IExternalServiceClient<string>> _externalServiceClients;
        private readonly ILogger<LyricsScraperClient> _logger;

        public bool IsEnabled => _externalServiceClients != null && _externalServiceClients.Any(x => x.IsEnabled);

        public LyricsScraperClient(ILogger<LyricsScraperClient> logger)
        {
            _logger = logger;
        }

        public LyricsScraperClient(ILogger<LyricsScraperClient> logger,
            IEnumerable<IExternalServiceClient<string>> externalServiceClients)
        {
            _logger = logger;

            Ensure.ArgumentNotNullOrEmptyList(externalServiceClients, nameof(externalServiceClients));
            _externalServiceClients = externalServiceClients.ToList();
        }

        public string SearchLyric(SearchRequest searchRequest)
        {
            if (!ValidateRequest())
                return null;

            foreach (var lyricClient in _externalServiceClients)
            {
                var lyric = lyricClient.SearchLyric(searchRequest);
                if (!string.IsNullOrEmpty(lyric))
                {
                    return lyric;
                }
                _logger?.LogWarning($"Can't find lyric by client: {lyricClient}.");
            }
            _logger?.LogError($"Can't find lyrics for searchRequest: {searchRequest}.");
            return null;
        }

        public async Task<string> SearchLyricAsync(SearchRequest searchRequest)
        {
            if (!ValidateRequest())
                return null;

            foreach (var lyricClient in _externalServiceClients)
            {
                var lyric = await lyricClient.SearchLyricAsync(searchRequest);
                if (!string.IsNullOrEmpty(lyric))
                {
                    return lyric;
                }
                _logger?.LogWarning($"Can't find lyric by client: {lyricClient}.");
            }
            _logger?.LogError($"Can't find lyrics for searchRequest: {searchRequest}.");
            return null;
        }

        private bool ValidateRequest()
        {
            string error = string.Empty;
            LogLevel logLevel = LogLevel.Error;

            if (IsEmptyClients())
            {
                error = "Empty client list! Please set any external client first.";
            }
            else if (!IsEnabled)
            {
                error = "All external clients is disabled. Searching lyrics is disabled.";
                logLevel = LogLevel.Warning;
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                _logger?.Log(logLevel, error);
                return false;
            }    
            return true;
        }

        public void AddClient(IExternalServiceClient<string> client)
        {
            if (IsEmptyClients())
                _externalServiceClients = new List<IExternalServiceClient<string>>();
            _externalServiceClients.Add(client);
        }

        private bool IsEmptyClients() => _externalServiceClients == null || !_externalServiceClients.Any();

    }
}