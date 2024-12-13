using HtmlAgilityPack;
using LyricsScraperNET.Network.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LyricsScraperNET.Network
{
    internal sealed class LyricFindWebClient : IWebClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LyricFindWebClient> _logger;

        public LyricFindWebClient()
        {
            _httpClient = new HttpClient();
        }

        public LyricFindWebClient(ILogger<LyricFindWebClient> logger) : this()
        {
            _logger = logger;
        }

        public string Load(Uri uri)
        {
            return LoadAsync(uri).GetAwaiter().GetResult();
        }

        public async Task<string> LoadAsync(Uri uri)
        {
            // Create the initial request
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            AddInitialHeaders(request);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogWarning($"Error loading URI: {uri}, Exception: {ex}");
                return string.Empty;
            }

            // If response is 202, send a follow-up request with modified headers
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                var secondRequest = new HttpRequestMessage(HttpMethod.Get, uri);
                AddFollowUpHeaders(secondRequest, uri.ToString());

                try
                {
                    response = await _httpClient.SendAsync(secondRequest);
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException ex)
                {
                    _logger?.LogWarning($"Error during follow-up request to URI: {uri}, Exception: {ex}");
                    return string.Empty;
                }
            }

            var htmlContent = await response.Content.ReadAsStringAsync();

            // Load the HTML content into an HtmlDocument
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            // Perform custom checks or modifications on the document
            CheckDocument(htmlDocument, uri);

            return htmlDocument.ParsedText;
        }

        private void AddInitialHeaders(HttpRequestMessage request)
        {
            // Add headers for the first request
            request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.45 Safari/537.36");
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("Pragma", "no-cache");
            request.Headers.Add("Sec-Fetch-Dest", "document");
            request.Headers.Add("Sec-Fetch-Mode", "navigate");
            request.Headers.Add("Sec-Fetch-Site", "none");
            request.Headers.Add("Sec-Fetch-User", "?1");
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
        }

        private void AddFollowUpHeaders(HttpRequestMessage request, string referer)
        {
            // Add headers for the second request
            request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.45 Safari/537.36");
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("Pragma", "no-cache");
            request.Headers.Add("Sec-Fetch-Dest", "document");
            request.Headers.Add("Sec-Fetch-Mode", "navigate");
            request.Headers.Add("Sec-Fetch-Site", "same-origin");
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            request.Headers.Add("Referer", referer); // Set the referer for the second request
        }

        private void CheckDocument(HtmlDocument document, Uri uri)
        {
            if (document == null || string.IsNullOrEmpty(document.ParsedText))
            {
                Console.WriteLine($"Document is invalid for URI: {uri}");
            }
        }
    }
}
