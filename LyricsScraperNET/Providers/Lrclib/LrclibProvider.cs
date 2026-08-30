using LyricsScraperNET.Extensions;
using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.Lrclib
{
    public sealed class LrclibProvider : ExternalProviderBase
    {
        private ILogger<LrclibProvider>? _logger;
        private readonly IExternalUriConverter _uriConverter;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        #region Constructors

        public LrclibProvider()
        {
            Parser = new LrclibParser();
            WebClient = new LrclibHttpClient();
            Options = new LrclibOptions() { Enabled = true };
            _uriConverter = new LrclibUriConverter();
        }

        public LrclibProvider(ILogger<LrclibProvider> logger, LrclibOptions options)
            : this()
        {
            _logger = logger;
            Ensure.ArgumentNotNull(options, nameof(options));
            Options = options;
        }

        public LrclibProvider(ILogger<LrclibProvider> logger, IOptionsSnapshot<LrclibOptions> options)
            : this(logger, options.Value)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        public LrclibProvider(LrclibOptions options)
            : this(NullLogger<LrclibProvider>.Instance, options)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        public LrclibProvider(IOptionsSnapshot<LrclibOptions> options)
            : this(NullLogger<LrclibProvider>.Instance, options.Value)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        #endregion

        public override IExternalProviderOptions Options { get; }

        #region Sync

        protected override SearchResult SearchLyric(string artist, string song, CancellationToken cancellationToken = default)
        {
            return SearchLyricAsync(artist, song, cancellationToken).GetAwaiter().GetResult();
        }

        protected override SearchResult SearchLyric(Uri uri, CancellationToken cancellationToken = default)
        {
            return SearchLyricAsync(uri, cancellationToken).GetAwaiter().GetResult();
        }

        #endregion

        #region Async

        protected override async Task<SearchResult> SearchLyricAsync(string artist, string song, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await SearchLyricAsync(_uriConverter.GetLyricUri(artist, song), cancellationToken);
        }

        protected override async Task<SearchResult> SearchLyricAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            if (WebClient == null || Parser == null)
            {
                _logger?.LogWarning($"Lrclib. Please set up WebClient and Parser first");
                return new SearchResult(ExternalProviderType.Lrclib);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var text = await WebClient.LoadAsync(uri, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return PostProcessLyric(uri, text);
        }

        #endregion

        public override void WithLogger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LrclibProvider>();
        }

        private SearchResult PostProcessLyric(Uri uri, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger?.LogWarning($"Lrclib. Response is empty for Uri: [{uri}]");
                return new SearchResult(ExternalProviderType.Lrclib);
            }

            LrclibTrackResponse? response;
            try
            {
                response = JsonSerializer.Deserialize<LrclibTrackResponse>(text, JsonOptions);
            }
            catch (JsonException ex)
            {
                _logger?.LogWarning($"Lrclib. Failed to parse JSON for Uri: [{uri}]. Exception: {ex}");
                return new SearchResult(ExternalProviderType.Lrclib);
            }

            if (response == null)
            {
                _logger?.LogWarning($"Lrclib. Empty parsed response for Uri: [{uri}]");
                return new SearchResult(ExternalProviderType.Lrclib);
            }

            if (response.Code == 404 || string.Equals(response.Name, "TrackNotFound", StringComparison.OrdinalIgnoreCase))
            {
                _logger?.LogInformation($"Lrclib. Track not found for Uri: [{uri}]");
                return new SearchResult(ExternalProviderType.Lrclib, ResponseStatusCode.NoDataFound);
            }

            if (response.Instrumental)
                return new SearchResult(ExternalProviderType.Lrclib).AddInstrumental(true);

            if (string.IsNullOrWhiteSpace(response.PlainLyrics))
            {
                _logger?.LogWarning($"Lrclib. Can't find lyrics for Uri: [{uri}]");
                return new SearchResult(ExternalProviderType.Lrclib);
            }

            var result = Parser.Parse(response.PlainLyrics);
            return new SearchResult(result, ExternalProviderType.Lrclib);
        }
    }
}
