using LyricsScraperNET.External.Abstract;
using LyricsScraperNET.Models;
using Microsoft.Extensions.Logging;

namespace LyricsScraper
{
    public sealed class LyricsScraperClient: ILyricsScraperClient<string>
    {
        private List<IExternalServiceClient<string>> _lyricClients;
        private readonly ILogger<LyricsScraperClient> _logger;

        public LyricsScraperClient(ILogger<LyricsScraperClient> logger)
        {
            _logger = logger;
        }

        public string SearchLyric(SearchRequest searchRequest)
        {
            if (IsEmptyClients())
            {
                _logger.LogError("Empty client list! Please set any external client first.");
            }
            foreach (var lyricClient in _lyricClients)
            {
                var lyric = lyricClient.SearchLyric(searchRequest);
                if (!string.IsNullOrEmpty(lyric))
                {
                    return lyric;
                }
                _logger.LogWarning($"Can't find lyric by client: {lyricClient}.");
            }
            _logger.LogError($"Can't find lyrics for searchRequest: {searchRequest}.");
            return null;
        }

        public async Task<string> SearchLyricAsync(SearchRequest searchRequest)
        {
            if (IsEmptyClients())
            {
                _logger.LogError("Empty client list! Please set any external client first.");
            }
            foreach (var lyricClient in _lyricClients)
            {
                var lyric = await lyricClient.SearchLyricAsync(searchRequest);
                if (!string.IsNullOrEmpty(lyric))
                {
                    return lyric;
                }
                _logger.LogWarning($"Can't find lyric by client: {lyricClient}.");
            }
            _logger.LogError($"Can't find lyrics for searchRequest: {searchRequest}.");
            return null;
        }

        public void AddClient(IExternalServiceClient<string> client)
        {
            if (IsEmptyClients())
                _lyricClients = new List<IExternalServiceClient<string>>();
            _lyricClients.Add(client);
        }

        private bool IsEmptyClients() => _lyricClients == null || !_lyricClients.Any();

    }
}