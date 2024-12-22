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
        private readonly HtmlWeb _htmlWeb;

        public HtmlAgilityWebClient()
        {
            _htmlWeb = new HtmlWeb();
            _htmlWeb.UsingCache = false;
        }

        public HtmlAgilityWebClient(ILogger<HtmlAgilityWebClient> logger) : this()
        {
            _logger = logger;
        }

        public string Load(Uri uri, CancellationToken cancellationToken = default)
        {
            return LoadAsync(uri, cancellationToken).GetAwaiter().GetResult();
        }

        public async Task<string> LoadAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            HtmlDocument document;
            try
            {
                document = await _htmlWeb.LoadFromWebAsync(uri.ToString(), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error loading document for uri: {uri}. Exception: {ex}");
                return string.Empty;
            }

            CheckDocument(document, uri);
            return document?.ParsedText;
        }

        private void CheckDocument(HtmlDocument document, Uri uri)
        {
            if (document == null)
            {
                _logger?.LogWarning($"HtmlPage could not load document for uri: {uri}");
                throw new InvalidOperationException($"Failed to load document for URI: {uri}");
            }

            if (string.IsNullOrWhiteSpace(document.ParsedText))
            {
                _logger?.LogWarning($"Document loaded for uri: {uri} but text is empty.");
                throw new InvalidOperationException($"Document loaded for URI: {uri}, but the text is empty.");
            }
        }
    }
}
