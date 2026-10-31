using HtmlAgilityPack;
using LyricsScraperNET.Extensions;
using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.Genius
{
    public sealed class GeniusProvider : ExternalProviderBase
    {
        private ILogger<GeniusProvider>? _logger;
        private readonly GeniusUriConverter _uriConverter;

        // Lyrics header. Note: Should be skipped in result
        private const string LyricsHeaderNodesXPath = "//div[contains(@class, 'LyricsHeader')]";

        private const string LyricsContainerNodesXPath = "//div[@data-lyrics-container]";

        // In case of instrumental song without a lyric.
        private const string LyricsPlaceholderNodesXPath = "//div[contains(@class, 'LyricsPlaceholder')]";
        private const string InstrumentalLyricText = "This song is an instrumental";

        // Straight, typographic and modifier letter apostrophes, backtick.
        private const string Apostrophes = "'\u2019\u2018`\u02BC";

        #region Constructors

        public GeniusProvider()
        {
            Parser = new GeniusParser();
            WebClient = CreateWebClient(SharedHttpClient.Instance);
            Options = new GeniusOptions() { Enabled = true };
            _uriConverter = new GeniusUriConverter();
        }

        public GeniusProvider(ILogger<GeniusProvider> logger, GeniusOptions options)
            : this()
        {
            _logger = logger;
            Ensure.ArgumentNotNull(options, nameof(options));
            Options = options;
        }

        public GeniusProvider(ILogger<GeniusProvider> logger, IOptionsSnapshot<GeniusOptions> options)
            : this(logger, options.Value)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        public GeniusProvider(GeniusOptions options)
            : this(NullLogger<GeniusProvider>.Instance, options)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        public GeniusProvider(IOptionsSnapshot<GeniusOptions> options)
            : this(NullLogger<GeniusProvider>.Instance, options.Value)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        #endregion

        public override IExternalProviderOptions Options { get; }

        #region Sync

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

        protected override async Task<SearchResult> SearchLyricAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            var htmlPageBody = await WebClient.LoadAsync(uri, cancellationToken);

            return GetSearchResultFromHtmlPageBody(uri, htmlPageBody);
        }

        protected override async Task<SearchResult> SearchLyricAsync(string artist, string song, CancellationToken cancellationToken = default)
        {
            var lyricUri = await FindLyricUriAsync(artist, song, cancellationToken);

            return lyricUri != null
                ? await SearchLyricAsync(lyricUri, cancellationToken)
                : new SearchResult(ExternalProviderType.Genius);
        }

        #endregion

        public override void WithLogger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GeniusProvider>();
        }

        /// <summary>
        /// Requests to the official API get the API key. It is read on every request, so a key set after construction is used too,
        /// and it is never sent to other hosts.
        /// </summary>
        private protected override IWebClient CreateWebClient(HttpClient httpClient)
            => new NetHttpClient(httpClient, configureRequest: AddApiKey);

        private void AddApiKey(HttpRequestMessage request)
        {
            if (string.Equals(request.RequestUri?.Host, GeniusUriConverter.ApiHost, StringComparison.OrdinalIgnoreCase)
                && Options.TryGetApiKeyFromOptions(out var apiKey))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        private async Task<Uri?> FindLyricUriAsync(string artist, string song, CancellationToken cancellationToken)
        {
            if (Options.TryGetApiKeyFromOptions(out _))
            {
                var apiSearchResponse = await WebClient.LoadAsync(_uriConverter.GetApiSearchUri(artist, song), cancellationToken);

                // Only a failed API request falls back to the public search: a valid answer without a match is final.
                if (TryGetLyricUriFromSearchResponse(apiSearchResponse, artist, song, out var apiLyricUri))
                    return apiLyricUri;

                _logger?.LogWarning("Genius. API search failed for artist {Artist} and song {Song}. Using the public search.", artist, song);
            }

            var searchResponse = await WebClient.LoadAsync(_uriConverter.GetLyricUri(artist, song), cancellationToken);
            TryGetLyricUriFromSearchResponse(searchResponse, artist, song, out var lyricUri);
            return lyricUri;
        }

        /// <summary>
        /// Finds the hit whose primary artist and title match the request.
        /// Returns false if <paramref name="searchResponse"/> is not a search response, for example an empty body or an error page.
        /// </summary>
        private bool TryGetLyricUriFromSearchResponse(string searchResponse, string artist, string song, out Uri? lyricUri)
        {
            lyricUri = null;

            if (string.IsNullOrWhiteSpace(searchResponse))
                return false;

            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(searchResponse);
            }
            catch (JsonException ex)
            {
                _logger?.LogWarning(ex, "Genius. Search response is not JSON.");
                return false;
            }

            using (document)
            {
                if (!TryGetProperty(document.RootElement, "response", JsonValueKind.Object, out var responseElement)
                    || !TryGetProperty(responseElement, "hits", JsonValueKind.Array, out var hitsElement))
                {
                    _logger?.LogWarning("Genius. Search response has no hits.");
                    return false;
                }

                foreach (var hitElement in hitsElement.EnumerateArray())
                {
                    if (!TryGetProperty(hitElement, "result", JsonValueKind.Object, out var resultElement)
                        || !TryGetProperty(resultElement, "primary_artist", JsonValueKind.Object, out var artistElement)
                        || !IsSameText(GetString(artistElement, "name"), artist)
                        || !IsSameText(GetString(resultElement, "title"), song))
                        continue;

                    var url = GetString(resultElement, "url");
                    if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                        lyricUri = uri;
                    else
                        _logger?.LogWarning("Genius. Invalid lyric url {Url} for artist {Artist} and song {Song}.", url, artist, song);

                    return true;
                }

                _logger?.LogInformation("Genius. Can't find artist {Artist} and song {Song} in the search response.", artist, song);
                return true;
            }
        }

        private SearchResult GetSearchResultFromHtmlPageBody(Uri uri, string htmlPageBody)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlPageBody);
            var rootNode = htmlDocument.DocumentNode;

            // Lyric's header node with additional information should be skipped.
            foreach (var headerNode in SelectNodes(rootNode, LyricsHeaderNodesXPath))
                headerNode.Remove();

            var lyricNodes = SelectNodes(rootNode, LyricsContainerNodesXPath).ToList();
            if (lyricNodes.Count == 0)
            {
                // No lyric containers in case of instrumental.
                if (SelectNodes(rootNode, LyricsPlaceholderNodesXPath).Any(node => node.InnerText.Contains(InstrumentalLyricText)))
                    return new SearchResult(ExternalProviderType.Genius).AddInstrumental(true);

                _logger?.LogWarning("Genius. Can't find lyrics for Uri: [{Uri}]", uri);
                return new SearchResult(ExternalProviderType.Genius);
            }

            var lyric = Parser.Parse(string.Concat(lyricNodes.Select(node => node.InnerHtml)));
            if (string.IsNullOrWhiteSpace(lyric))
            {
                _logger?.LogWarning("Genius. Lyrics are empty for Uri: [{Uri}]", uri);
                return new SearchResult(ExternalProviderType.Genius);
            }

            return new SearchResult(lyric, ExternalProviderType.Genius);
        }

        private static IEnumerable<HtmlNode> SelectNodes(HtmlNode node, string xPath)
            => node.SelectNodes(xPath) ?? Enumerable.Empty<HtmlNode>();

        private static bool TryGetProperty(JsonElement element, string propertyName, JsonValueKind valueKind, out JsonElement value)
        {
            value = default;
            return element.ValueKind == JsonValueKind.Object
                && element.TryGetProperty(propertyName, out value)
                && value.ValueKind == valueKind;
        }

        private static string? GetString(JsonElement element, string propertyName)
            => TryGetProperty(element, propertyName, JsonValueKind.String, out var value) ? value.GetString() : null;

        /// <summary>
        /// Compares letters and digits only, ignoring case and diacritics. Examples from Genius:
        /// "Beyonce" = "Beyoncé", "Jay-Z" = "JAŸ-Z", "Guns N Roses" = "Guns N’ Roses",
        /// "Harder, Better, Faster, Stronger" = "Harder Better Faster Stronger", "Rock &amp; Roll" = "Rock and Roll",
        /// "What Happens If I Cant Check My MySpace" = "What Happens If I Can’t Check My MySpace?".
        /// Texts without letters and digits (for example the artist "!!!") are compared as they are.
        /// </summary>
        private static bool IsSameText(string? actual, string expected)
        {
            if (actual == null)
                return false;

            var normalizedActual = NormalizeText(actual);
            var normalizedExpected = NormalizeText(expected);
            if (normalizedActual.Length == 0 || normalizedExpected.Length == 0)
                return string.Equals(actual.Trim(), expected.Trim(), StringComparison.OrdinalIgnoreCase);

            return string.Equals(normalizedActual, normalizedExpected, StringComparison.Ordinal);
        }

        private static string NormalizeText(string text)
        {
            var builder = new StringBuilder(text.Length);
            foreach (var c in text.Replace("&", " and ").Normalize(NormalizationForm.FormD))
            {
                if (char.IsLetterOrDigit(c))
                    builder.Append(char.ToLowerInvariant(c));
                // Apostrophes join the word parts: "Don’t" = "Dont". Diacritics are dropped: "é" = "e".
                else if (Apostrophes.Contains(c)
                    || CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                    continue;
                else
                    builder.Append(' ');
            }

            // Genius writes parts as "Pt.": "Dr. Shavargo, Pt. 3" = "Dr. Shavargo Part 3".
            return string.Join(" ", builder.ToString()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(word => word == "pt" ? "part" : word));
        }
    }
}
