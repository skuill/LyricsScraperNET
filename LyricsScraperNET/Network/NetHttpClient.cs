using LyricsScraperNET.Network.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LyricsScraperNET.Network
{
    internal sealed class NetHttpClient : IWebClient
    {
        private readonly ILogger<NetHttpClient> _logger;

        public NetHttpClient()
        {
        }

        public NetHttpClient(ILogger<NetHttpClient> logger) : this()
        {
            _logger = logger;
        }

        public string Load(Uri uri)
        {
            return LoadAsync(uri).GetAwaiter().GetResult();
        }

        public async Task<string> LoadAsync(Uri uri)
        {
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            HttpResponseMessage response;
            try
            {
                response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogWarning($"HttpClient GetStringAsync throw exception for uri: {uri}. Exception: {ex}");
                return string.Empty;
            }

            var htmlContent = await response.Content.ReadAsStringAsync();

            CheckResult(htmlContent, uri);

            return htmlContent;
        }

        private void CheckResult(string? result, Uri uri)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                _logger?.LogDebug($"HttpClient GetString return null for uri: {uri}");
            }
        }
    }
}
