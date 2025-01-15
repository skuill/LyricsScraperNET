using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;
using HtmlAgilityPack;

namespace LyricsScraperNET.Providers.KPopLyrics
{
    public sealed class KPopLyricsProvider : ExternalProviderBase
    {
        private ILogger<KPopLyricsProvider>? _logger;
        private readonly IExternalUriConverter _uriConverter;

        private const string LyricsContainerNodesXPath = "//*[contains(@class, 'entry-content') and contains(@class, 'mh-clearfix')]";
        #region Constructors

        public KPopLyricsProvider()
        {
            Parser = new KPopLyricsParser();
            WebClient = new HtmlAgilityWebClient();
            Options = new KPopLyricsOptions() { Enabled = true };
            _uriConverter = new KPopLyricsUriConverter();
        }

        public KPopLyricsProvider(ILogger<KPopLyricsProvider> logger, KPopLyricsOptions options)
            : this()
        {
            _logger = logger;
            Ensure.ArgumentNotNull(options, nameof(options));
            Options = options;
        }

        public KPopLyricsProvider(ILogger<KPopLyricsProvider> logger, IOptionsSnapshot<KPopLyricsOptions> options)
            : this(logger, options.Value)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        public KPopLyricsProvider(KPopLyricsOptions options)
            : this(NullLogger<KPopLyricsProvider>.Instance, options)
        {
            Ensure.ArgumentNotNull(options, nameof(options));
        }

        public KPopLyricsProvider(IOptionsSnapshot<KPopLyricsOptions> options)
            : this(NullLogger<KPopLyricsProvider>.Instance, options.Value)
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
            cancellationToken.ThrowIfCancellationRequested(); // Ensure cancellation is handled early
            return await SearchLyricAsync(_uriConverter.GetLyricUri(artist, song), cancellationToken);
        }

        protected override async Task<SearchResult> SearchLyricAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            if (WebClient == null || Parser == null)
            {
                _logger?.LogWarning($"KPopLyrics. Please set up WebClient and Parser first");
                return new SearchResult(ExternalProviderType.KPopLyrics);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var text = await WebClient.LoadAsync(uri, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return PostProcessLyric(uri, text);
        }

        #endregion

        public override void WithLogger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<KPopLyricsProvider>();
        }
        private SearchResult PostProcessLyric(Uri uri, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                _logger?.LogWarning($"KPopLyrics. Text is empty for Uri: [{uri}]");
                return new SearchResult(ExternalProviderType.KPopLyrics);
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(text);

            var mainNode = htmlDoc.DocumentNode.SelectNodes(LyricsContainerNodesXPath).FirstOrDefault();

            if (mainNode is null)
            {
                _logger?.LogWarning($"KPopLyrics. Can't parse lyric from the page. Uri: {uri}");
                return new SearchResult(ExternalProviderType.KPopLyrics, ResponseStatusCode.NoDataFound);
            }

            var h2Nodes = htmlDoc.DocumentNode.SelectNodes("//h2");

            if (h2Nodes is null || !h2Nodes.Any())
            {
                _logger?.LogWarning($"KPopLyrics. Can't parse lyric from the page. Uri: {uri}");
                return new SearchResult(ExternalProviderType.KPopLyrics, ResponseStatusCode.NoDataFound);
            }

            // sometimes lyrics have eng translation but sometimes its only romanized version.
            var h2Node = h2Nodes.FirstOrDefault(x => x.OuterHtml.Contains("Official English Translation")) ?? h2Nodes.FirstOrDefault(x => x.OuterHtml.Contains("Romanized"));

            if (h2Node is null)
            {
                _logger?.LogWarning($"KPopLyrics. Can't parse lyric from the page. Uri: {uri}");
                return new SearchResult(ExternalProviderType.KPopLyrics, ResponseStatusCode.NoDataFound);
            }

            var rawHtmlLyrics = TakeParagraphsUntilHeader(h2Node);

            if (string.IsNullOrEmpty(rawHtmlLyrics))
            {
                _logger?.LogWarning($"KPopLyrics. Can't parse lyric from the page. Uri: {uri}");
                return new SearchResult(ExternalProviderType.KPopLyrics, ResponseStatusCode.NoDataFound);
            }

            var result = Parser.Parse(rawHtmlLyrics);

            return new SearchResult(result, ExternalProviderType.KPopLyrics);
        }

        private string TakeParagraphsUntilHeader(HtmlNode startNode)
        {
            var paragraphs = new List<string>();

            var sibling = startNode.NextSibling;
            while (sibling != null)
            {
                if (sibling.Name == "h2")
                {
                    break;
                }

                if (sibling.Name == "p")
                {
                    paragraphs.Add(sibling.OuterHtml);
                }

                if (sibling.Name != "h2" && sibling.Name != "p")
                {
                    break;
                }

                sibling = sibling.NextSibling;
            }

            return string.Join("\n", paragraphs);
        }
    }
}