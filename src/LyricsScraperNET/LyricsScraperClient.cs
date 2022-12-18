using LyricsScraper.Abstract;
using Microsoft.Extensions.Logging;

namespace LyricsScraper
{
    public class LyricsScraperClient: ILyricsScraperClient<string>
    {
        private List<ILyricClient<string>> _lyricClients;
        private readonly ILogger<LyricsScraperClient> _logger;

        public LyricsScraperClient(ILogger<LyricsScraperClient> logger)
        {
            _logger = logger;
        }

        public string SearchLyric(Uri uri)
        {
            if (IsEmptyClients())
            {
                _logger.LogError("Empty client list! Please set any external client first.");
            }
            foreach (var lyricClient in _lyricClients)
            {
                var lyric = lyricClient.SearchLyric(uri);
                if (!string.IsNullOrEmpty(lyric))
                {
                    return lyric;
                }
                _logger.LogWarning($"Can't find lyric by client: {lyricClient}.");
            }
            _logger.LogError($"Can't find lyrics for {uri}.");
            return null;
        }

        public string SearchLyric(string artist, string song)
        {
            if (IsEmptyClients())
            {
                _logger.LogError("Empty client list! Please set any external client first.");
            }
            foreach (var lyricClient in _lyricClients)
            {
                var lyric = lyricClient.SearchLyric(artist, song);
                if (!string.IsNullOrEmpty(lyric))
                {
                    return lyric;
                }
                _logger.LogWarning($"Can't find lyric by client: {lyricClient}.");
            }
            _logger.LogError($"Can't find lyrics! Artist: {artist}. Song: {song}");
            return null;
        }

        public async Task<string> SearchLyricAsync(Uri uri)
        {
            if (IsEmptyClients())
            {
                _logger.LogError("Empty client list! Please set any external client first.");
            }
            foreach (var lyricClient in _lyricClients)
            {
                var lyric = await lyricClient.SearchLyricAsync(uri);
                if (!string.IsNullOrEmpty(lyric))
                {
                    return lyric;
                }
                _logger.LogWarning($"Can't find lyric by client: {lyricClient}.");
            }
            _logger.LogError($"Can't find lyrics for {uri}.");
            return null;
        }

        public async Task<string> SearchLyricAsync(string artist, string song)
        {
            if (IsEmptyClients())
            {
                _logger.LogError("Empty client list! Please set any external client first.");
            }
            foreach (var lyricClient in _lyricClients)
            {
                var lyric = await lyricClient.SearchLyricAsync(artist, song);
                if (!string.IsNullOrEmpty(lyric))
                {
                    return lyric;
                }
                _logger.LogWarning($"Can't find lyric by client: {lyricClient}.");
            }
            _logger.LogError($"Can't find lyrics! Artist: {artist}. Song: {song}");
            return null;
        }

        public void AddClient(ILyricClient<string> client)
        {
            if (IsEmptyClients())
                _lyricClients = new List<ILyricClient<string>>();
            _lyricClients.Add(client);
        }

        private bool IsEmptyClients() => _lyricClients == null || !_lyricClients.Any();

    }
}