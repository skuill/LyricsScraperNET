using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Abstract;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusixmatchClientLib;
using MusixmatchClientLib.API.Model.Exceptions;
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

        // Musixmatch Token memory cache
        private static readonly IMemoryCache _memoryCache;
        private static readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;

        private static readonly string MusixmatchTokenKey = "MusixmatchToken";

        private readonly int _searchRetryAmount = 2;

        #region Constructors

        static MusixmatchProvider()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions()
            {
                SizeLimit = 1024,
            });
            _memoryCacheEntryOptions = new MemoryCacheEntryOptions()
            {
                Size = 1
            };
        }

        public MusixmatchProvider()
        {
            Options = new MusixmatchOptions() { Enabled = true };
        }

        public MusixmatchProvider(ILogger<MusixmatchProvider> logger, MusixmatchOptions options)
        {
            _logger = logger;
            Ensure.ArgumentNotNull(options, nameof(options));
            Options = options;
        }

        public MusixmatchProvider(ILogger<MusixmatchProvider> logger, IOptionsSnapshot<MusixmatchOptions> options)
            : this(logger, options.Value)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        #endregion

        public override IExternalProviderOptions Options { get; }

        #region Sync

        // TODO: search by uri from the site. Example: https://www.musixmatch.com/lyrics/Parkway-Drive/Idols-and-Anchors
        protected override SearchResult SearchLyric(Uri uri)
        {
            return new SearchResult();
        }

        protected override SearchResult SearchLyric(string artist, string song)
        {
            int? trackId;
            bool regenerateToken = false;
            for (int i = 1; i <= _searchRetryAmount; i++)
            {
                var client = GetMusixmatchClient(regenerateToken);
                var trackSearchParameters = GetTrackSearchParameters(artist, song);
                try
                {
                    trackId = client.SongSearch(trackSearchParameters)?.FirstOrDefault()?.TrackId;
                    if (trackId != null)
                    {
                        Lyrics lyrics = client.GetTrackLyrics(trackId.Value);
                        return lyrics.Instrumental != 1
                            ? new SearchResult(lyrics.LyricsBody, Models.ExternalProviderType.Musixmatch)
                            : new SearchResult(); // lyrics.LyricsBody is null when the track is instrumental
                    }
                    else
                    {
                        _logger?.LogWarning($"Musixmatch. Can't find any information about artist {artist} and song {song}");
                        return new SearchResult();
                    }
                }
                catch (MusixmatchRequestException requestException) when (requestException.StatusCode == StatusCode.AuthFailed)
                {
                    _logger?.LogWarning($"Musixmatch. Authentication failed. Error: {requestException.Message}.");
                    regenerateToken = true;
                }
            }
            return new SearchResult();
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
            bool regenerateToken = false;
            for (int i = 1; i <= _searchRetryAmount; i++)
            {
                var client = GetMusixmatchClient(regenerateToken);
                var trackSearchParameters = GetTrackSearchParameters(artist, song);
                try
                {
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
                        _logger?.LogWarning($"Musixmatch. Can't find any information about artist {artist} and song {song}");
                        return new SearchResult();
                    }
                }
                catch (MusixmatchRequestException requestException) when (requestException.StatusCode == StatusCode.AuthFailed)
                {
                    _logger?.LogWarning($"Musixmatch. Authentication failed. Error: {requestException.Message}.");
                    regenerateToken = true;
                }
            }
            return new SearchResult();
        }

        #endregion

        private MusixmatchClient GetMusixmatchClient(bool regenerateToken = false)
        {
            // TODO: uncomment after the fix of https://github.com/Eimaen/MusixmatchClientLib/issues/21
            //if (Options.TryGetApiKeyFromOptions(out var apiKey))
            //{
            //    _logger.LogInformation("Use MusixmatchToken from options.");
            //    return new MusixmatchClient(apiKey);
            //}
            //else
            //{
            if (regenerateToken)
                _memoryCache.Remove(MusixmatchTokenKey);

            _logger?.LogDebug("Musixmatch. Use default MusixmatchToken.");
            string musixmatchTokenValue;
            if (!_memoryCache.TryGetValue(MusixmatchTokenKey, out musixmatchTokenValue))
            {
                _logger?.LogDebug("Musixmatch. Generate new token.");
                var musixmatchToken = new MusixmatchToken();
                musixmatchTokenValue = musixmatchToken.Token;
                _memoryCache.Set(MusixmatchTokenKey, musixmatchTokenValue, _memoryCacheEntryOptions);
            }
            (Options as IExternalProviderOptionsWithApiKey).ApiKey = musixmatchTokenValue;
            return new MusixmatchClient(musixmatchTokenValue);
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
    }
}
