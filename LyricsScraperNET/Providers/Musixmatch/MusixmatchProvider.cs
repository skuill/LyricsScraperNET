using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Abstract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MusixmatchClientLib.API.Model.Exceptions;
using MusixmatchClientLib.API.Model.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.Musixmatch
{
    public sealed class MusixmatchProvider : ExternalProviderBase
    {
        private ILogger<MusixmatchProvider>? _logger;
        private IMusixmatchClientWrapper _clientWrapper;

        private readonly int _searchRetryAmount = 2;

        #region Constructors

        public MusixmatchProvider()
        {
            Options = new MusixmatchOptions() { Enabled = true };

            var tokenCache = new MusixmatchTokenCache();
            _clientWrapper = new MusixmatchClientWrapper(tokenCache);
        }

        public MusixmatchProvider(ILogger<MusixmatchProvider> logger, MusixmatchOptions options, IMusixmatchClientWrapper clientWrapper)
            : this()
        {
            _logger = logger;
            Ensure.ArgumentNotNull(options, nameof(options));
            Options = options;
            Ensure.ArgumentNotNull(clientWrapper, nameof(clientWrapper));
            _clientWrapper = clientWrapper;
        }

        public MusixmatchProvider(ILogger<MusixmatchProvider> logger, IOptionsSnapshot<MusixmatchOptions> options, IMusixmatchClientWrapper clientWrapper)
            : this(logger, options.Value, clientWrapper)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        public MusixmatchProvider(MusixmatchOptions options, IMusixmatchClientWrapper clientWrapper)
            : this(NullLogger<MusixmatchProvider>.Instance, options, clientWrapper)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        public MusixmatchProvider(IOptionsSnapshot<MusixmatchOptions> options, IMusixmatchClientWrapper clientWrapper)
            : this(NullLogger<MusixmatchProvider>.Instance, options.Value, clientWrapper)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        #endregion

        public override IExternalProviderOptions Options { get; }

        #region Sync

        // TODO: search by uri from the site. Example: https://www.musixmatch.com/lyrics/Parkway-Drive/Idols-and-Anchors
        protected override SearchResult SearchLyric(Uri uri, CancellationToken cancellationToken = default)
        {
            return SearchLyricAsync(uri, cancellationToken).GetAwaiter().GetResult();
        }

        protected override SearchResult SearchLyric(string artist, string song, CancellationToken cancellationToken = default)
        {
            return SearchLyricAsync(artist, song, cancellationToken).GetAwaiter().GetResult();
        }

        #endregion

        #region Async

        // TODO: search by uri from the site. Example: https://www.musixmatch.com/lyrics/Parkway-Drive/Idols-and-Anchors
        protected override Task<SearchResult> SearchLyricAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<SearchResult>(new SearchResult(Models.ExternalProviderType.Musixmatch));
        }

        protected override async Task<SearchResult> SearchLyricAsync(string artist, string song, CancellationToken cancellationToken = default)
        {
            bool regenerateToken = false;
            for (int i = 1; i <= _searchRetryAmount; i++)
            {
                try
                {
                    var result = await _clientWrapper.SearchLyricAsync(artist, song, cancellationToken, regenerateToken);
                    return result;
                }
                catch (MusixmatchRequestException requestException) when (requestException.StatusCode == StatusCode.AuthFailed)
                {
                    _logger?.LogWarning($"Musixmatch. Authentication failed. Error: {requestException.Message}.");
                    regenerateToken = true;
                }
            }
            return new SearchResult(Models.ExternalProviderType.Musixmatch);
        }

        #endregion

        public override void WithLogger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<MusixmatchProvider>();
        }
    }
}
