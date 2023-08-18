using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Abstract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusixmatchClientLib;
using MusixmatchClientLib.API.Model.Types;
using MusixmatchClientLib.Auth;
using MusixmatchClientLib.Types;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.Musixmatch
{
    public sealed class MusixmatchProvider : ExternalProviderBase
    {
        private readonly ILogger<MusixmatchProvider> _logger;

        public MusixmatchProvider()
        {
            Options = new MusixmatchOptions() { Enabled = true };
        }

        public MusixmatchProvider(ILogger<MusixmatchProvider> logger, MusixmatchOptions musixmatchOptions)
        {
            _logger = logger;
            Ensure.ArgumentNotNull(musixmatchOptions, nameof(musixmatchOptions));
            Options = musixmatchOptions;
        }

        public MusixmatchProvider(ILogger<MusixmatchProvider> logger, IOptionsSnapshot<MusixmatchOptions> musixmatchOptions)
            : this(logger, musixmatchOptions.Value)
        {
            Ensure.ArgumentNotNull(musixmatchOptions, nameof(musixmatchOptions));
        }

        public override IExternalProviderOptions Options { get; }

        private MusixmatchClient GetMusixmatchClient()
        {
            // TODO: uncomment after the fix of https://github.com/Eimaen/MusixmatchClientLib/issues/21
            //if (Options.TryGetApiKeyFromOptions(out var apiKey))
            //{
            //    _logger.LogInformation("Use MusixmatchToken from options.");
            //    return new MusixmatchClient(apiKey);
            //}
            //else
            //{
            _logger?.LogInformation("Musixmatch. Use default MusixmatchToken.");
            var musixmatchToken = new MusixmatchToken();
            (Options as IExternalProviderOptionsWithApiKey).ApiKey = musixmatchToken.Token;
            return new MusixmatchClient(musixmatchToken);
            //}
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

        #region Sync

        // TODO: search by uri from the site. Example: https://www.musixmatch.com/lyrics/Parkway-Drive/Idols-and-Anchors
        protected override SearchResult SearchLyric(Uri uri)
        {
            return new SearchResult();
        }

        protected override SearchResult SearchLyric(string artist, string song)
        {
            var client = GetMusixmatchClient();
            var trackSearchParameters = GetTrackSearchParameters(artist, song);

            var trackId = client.SongSearch(trackSearchParameters)?.FirstOrDefault()?.TrackId;
            if (trackId != null)
            {
                Lyrics lyrics = client.GetTrackLyrics(trackId.Value);
                return lyrics.Instrumental != 1
                    ? new SearchResult(lyrics.LyricsBody, Models.ExternalProviderType.Musixmatch)
                    : new SearchResult(); // lyrics.LyricsBody is null when the track is instrumental
            }
            else
            {
                _logger?.LogError($"Musixmatch. Can't find any information about artist {artist} and song {song}");
                return new SearchResult();
            }
        }

        #endregion


        #region Async

        // TODO: search by uri from the site. Example: https://www.musixmatch.com/lyrics/Parkway-Drive/Idols-and-Anchors
        protected override Task<SearchResult> SearchLyricAsync(Uri uri)
        {
            return Task.FromResult<SearchResult>(new SearchResult());
        }

        protected override async Task<SearchResult> SearchLyricAsync(string artist, string song)
        {
            var client = GetMusixmatchClient();
            var trackSearchParameters = GetTrackSearchParameters(artist, song);

            var songSearchTask = await client.SongSearchAsync(trackSearchParameters);

            var trackId = songSearchTask?.FirstOrDefault()?.TrackId;
            if (trackId != null)
            {
                Lyrics lyrics = await client.GetTrackLyricsAsync(trackId.Value);
                return lyrics.Instrumental != 1
                    ? new SearchResult(lyrics.LyricsBody, Models.ExternalProviderType.Musixmatch)
                    : new SearchResult(); // lyrics.LyricsBody is null when the track is instrumental
            }
            else
            {
                _logger?.LogError($"Musixmatch. Can't find any information about artist {artist} and song {song}");
                return new SearchResult();
            }
        }

        #endregion
    }
}
