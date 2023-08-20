using HtmlAgilityPack;
using LyricsScraperNET.Network.Abstract;
using Microsoft.Extensions.Logging;
using System;
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

        public string Load(Uri uri)
        {
            var htmlPage = new HtmlWeb();
            var document = htmlPage.Load(uri, "GET");

            CheckDocument(document, uri);

            return document?.ParsedText;
        }

        public async Task<string> LoadAsync(Uri uri)
        {
            var htmlPage = new HtmlWeb();
            var document = await htmlPage.LoadFromWebAsync(uri.ToString());

            CheckDocument(document, uri);

            return document?.ParsedText;
        }

        private void CheckDocument(HtmlDocument document, Uri uri)
        {
            if (document == null)
            {
                _logger?.LogDebug($"HtmlPage Load return null for uri: {uri}");
            }
        }
    }
}
