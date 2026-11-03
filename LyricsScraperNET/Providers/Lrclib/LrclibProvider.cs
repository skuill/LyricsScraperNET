using LyricsScraperNET.Extensions;
using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
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

        private static readonly Regex AmpersandRegex = new(@"\s*&\s*", RegexOptions.Compiled);
        private static readonly Regex AndWordRegex = new(@"\band\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

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

        #region Async

        protected override async Task<SearchResult> SearchLyricAsync(string artist, string song, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (result, trackNotFound) = await SearchTrackAsync(_uriConverter.GetLyricUri(artist, song), cancellationToken).ConfigureAwait(false);

            // The server turns "&" into a space, so "Rock & Roll" does not find "Rock And Roll". The database keeps either spelling
            // ("Simon & Garfunkel" and "Simon and Garfunkel"), so the other one is tried once.
            if (trackNotFound && TrySwapAmpersand(artist, song, out var otherArtist, out var otherSong))
            {
                _logger?.LogInformation("Lrclib. Track not found, retrying with artist [{Artist}] and song [{Song}].", otherArtist, otherSong);
                (result, _) = await SearchTrackAsync(_uriConverter.GetLyricUri(otherArtist, otherSong), cancellationToken).ConfigureAwait(false);
            }

            return result;
        }

        protected override async Task<SearchResult> SearchLyricAsync(Uri uri, CancellationToken cancellationToken = default)
            => (await SearchTrackAsync(uri, cancellationToken).ConfigureAwait(false)).Result;

        #endregion

        public override void WithLogger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LrclibProvider>();
        }

        private protected override IWebClient CreateWebClient(HttpClient httpClient)
            => new LrclibHttpClient(httpClient);

        /// <returns>The search result and whether the server answered TrackNotFound (not an empty or broken response).</returns>
        private async Task<(SearchResult Result, bool TrackNotFound)> SearchTrackAsync(Uri uri, CancellationToken cancellationToken)
        {
            if (WebClient == null || Parser == null)
            {
                _logger?.LogWarning($"Lrclib. Please set up WebClient and Parser first");
                return (new SearchResult(ExternalProviderType.Lrclib), false);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var text = await LoadPageAsync(uri, cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            return PostProcessLyric(uri, text);
        }

        private (SearchResult Result, bool TrackNotFound) PostProcessLyric(Uri uri, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger?.LogWarning($"Lrclib. Response is empty for Uri: [{uri}]");
                return (new SearchResult(ExternalProviderType.Lrclib), false);
            }

            LrclibTrackResponse? response;
            try
            {
                response = JsonSerializer.Deserialize<LrclibTrackResponse>(text, JsonOptions);
            }
            catch (JsonException ex)
            {
                _logger?.LogWarning($"Lrclib. Failed to parse JSON for Uri: [{uri}]. Exception: {ex}");
                return (new SearchResult(ExternalProviderType.Lrclib), false);
            }

            if (response == null)
            {
                _logger?.LogWarning($"Lrclib. Empty parsed response for Uri: [{uri}]");
                return (new SearchResult(ExternalProviderType.Lrclib), false);
            }

            if (response.Code == 404 || string.Equals(response.Name, "TrackNotFound", StringComparison.OrdinalIgnoreCase))
            {
                _logger?.LogInformation($"Lrclib. Track not found for Uri: [{uri}]");
                return (new SearchResult(ExternalProviderType.Lrclib), true);
            }

            if (response.Instrumental)
                return (new SearchResult(ExternalProviderType.Lrclib).AsInstrumental(), false);

            if (string.IsNullOrWhiteSpace(response.PlainLyrics))
            {
                _logger?.LogWarning($"Lrclib. Can't find lyrics for Uri: [{uri}]");
                return (new SearchResult(ExternalProviderType.Lrclib), false);
            }

            var result = Parser.Parse(response.PlainLyrics);
            return (new SearchResult(result, ExternalProviderType.Lrclib), false);
        }

        /// <summary>
        /// "&amp;" becomes "and" when the request has it, otherwise the word "and" becomes "&amp;". False if the request has neither.
        /// </summary>
        internal static bool TrySwapAmpersand(string artist, string song, out string otherArtist, out string otherSong)
        {
            if (artist.Contains('&') || song.Contains('&'))
            {
                otherArtist = AmpersandRegex.Replace(artist, " and ").Trim();
                otherSong = AmpersandRegex.Replace(song, " and ").Trim();
            }
            else
            {
                otherArtist = AndWordRegex.Replace(artist, "&");
                otherSong = AndWordRegex.Replace(song, "&");
            }

            return !string.Equals(otherArtist, artist, StringComparison.Ordinal)
                || !string.Equals(otherSong, song, StringComparison.Ordinal);
        }
    }
}
