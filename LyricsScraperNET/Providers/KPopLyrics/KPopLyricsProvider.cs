using LyricsScraperNET.Helpers;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network;
using LyricsScraperNET.Providers.Abstract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using LyricsScraperNET.Providers.Models;

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
                return new SearchResult(Models.ExternalProviderType.KPopLyrics);
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
                return new SearchResult(ExternalProviderType.KPopLyrics, ResponseStatusCode.NoDataFound);
            }
            
            var h2Node = htmlDoc.DocumentNode.SelectNodes("//h2").FirstOrDefault();
            
            if (h2Node is null)
            {
                return new SearchResult(ExternalProviderType.KPopLyrics, ResponseStatusCode.NoDataFound);
            }

            var rawHtmlLyrics = TakeParagraphsUntilHeader(h2Node);
            
            if (string.IsNullOrEmpty(rawHtmlLyrics))
            {
                return new SearchResult(ExternalProviderType.KPopLyrics, ResponseStatusCode.NoDataFound);
            }

            var result = Parser.Parse(rawHtmlLyrics);

            return new SearchResult(result, ExternalProviderType.KPopLyrics);
        }

        // Page lyrics looks like:
        // h2 (Official English Translation)
        // p
        // p
        // p
        // h2 (Romanized)
        // p
        // p
        // p
        // p
        // we only want the first lyrics
        private string TakeParagraphsUntilHeader(HtmlNode h2)
        {
            var output = string.Empty;
            
            var sibling = h2.NextSibling;
            while (sibling != null)
            {
                if (sibling.Name == "h2")
                {
                    break;
                }

                if (sibling.Name == "p")
                {
                    output = string.Concat(output, sibling.OuterHtml);
                }

                sibling = sibling.NextSibling;
            }

            return output;
        }
    }
}