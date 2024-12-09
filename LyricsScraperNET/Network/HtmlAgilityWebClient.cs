using HtmlAgilityPack;
using LyricsScraperNET.Network.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.Network
{
    internal sealed class HtmlAgilityWebClient : IWebClient
    {
        private readonly ILogger<HtmlAgilityWebClient> _logger;

        public HtmlAgilityWebClient()
        {
        }

        public HtmlAgilityWebClient(ILogger<HtmlAgilityWebClient> logger)
        {
            _logger = logger;
        }

        public string Load(Uri uri, CancellationToken cancellationToken)
        {
            var htmlPage = new HtmlWeb();
            var document = htmlPage.Load(uri, "GET");

            CheckDocument(document, uri);

            return document?.ParsedText;
        }

        public async Task<string> LoadAsync(Uri uri, CancellationToken cancellationToken)
        {
            var htmlPage = new HtmlWeb();
            var document = await htmlPage.LoadFromWebAsync(uri.ToString(), cancellationToken);

            CheckDocument(document, uri);

            return document?.ParsedText;
        }

        private void CheckDocument(HtmlDocument document, Uri uri)
        {
            if (document == null)
            {
                _logger?.LogWarning($"HtmlPage could not load document for uri: {uri}");
            }
        }
    }
}
