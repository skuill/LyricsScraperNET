using LyricsScraperNET.Extensions;
using LyricsScraperNET.Models.Responses;
using Microsoft.Extensions.Logging;
using MusixmatchClientLib;
using MusixmatchClientLib.API.Model.Types;
using MusixmatchClientLib.Types;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.Musixmatch
{
    public sealed class MusixmatchClientWrapper : IMusixmatchClientWrapper
    {
        private ILogger<MusixmatchClientWrapper> _logger;
        private IMusixmatchTokenCache _tokenCache;

        public MusixmatchClientWrapper()
        {
        }

        public MusixmatchClientWrapper(IMusixmatchTokenCache tokenCache) : this()
        {
            _tokenCache = tokenCache;
        }

        public MusixmatchClientWrapper(ILogger<MusixmatchClientWrapper> logger, IMusixmatchTokenCache tokenCache)
            : this(tokenCache)
        {
            _logger = logger;
        }

        public SearchResult SearchLyric(string artist, string song, CancellationToken cancellationToken = default, bool regenerateToken = false)
        {
            return SearchLyricAsync(artist, song, cancellationToken).GetAwaiter().GetResult();
        }

        public async Task<SearchResult> SearchLyricAsync(string artist, string song, CancellationToken cancellationToken = default, bool regenerateToken = false)
        {
            var client = GetMusixmatchClient(regenerateToken);
            var trackSearchParameters = GetTrackSearchParameters(artist, song);

            var songSearchTask = await client.SongSearchAsync(trackSearchParameters);

            var trackId = songSearchTask?.FirstOrDefault()?.TrackId;
            if (trackId != null)
            {
                Lyrics lyrics = await client.GetTrackLyricsAsync(trackId.Value);

                // lyrics.LyricsBody is null when the track is instrumental
                if (lyrics.Instrumental != 1)
                    return new SearchResult(lyrics.LyricsBody, Models.ExternalProviderType.Musixmatch);

                // Instrumental music without lyric
                return new SearchResult(Models.ExternalProviderType.Musixmatch)
                    .AddInstrumental(true);
            }
            else
            {
                _logger?.LogWarning($"Musixmatch. Can't find any information about artist {artist} and song {song}");
                return new SearchResult(Models.ExternalProviderType.Musixmatch);
            }
        }

        private MusixmatchClient GetMusixmatchClient(bool regenerateToken = false)
        {
            var musixmatchToken = _tokenCache.GetOrCreateToken(regenerateToken);
            return new MusixmatchClient(musixmatchToken);
        }

        private TrackSearchParameters GetTrackSearchParameters(string artist, string song)
        {
            return new TrackSearchParameters
            {
                Artist = artist,
                Title = song, // Track name
                //Query = $"{artist} - {song}", // Search query, covers all the search parameters above
                //HasLyrics = false, // Only search for tracks with lyrics
                //HasSubtitles = false, // Only search for tracks with synced lyrics
                //Language = "", // Only search for tracks with lyrics in specified language
                Sort = TrackSearchParameters.SortStrategy.TrackRatingDesc // List sorting strategy 
            };
        }
    }
}
