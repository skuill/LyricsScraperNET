using LyricsScraperNET.Network.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LyricsScraperNET.Network
{
    internal sealed class NetHttpClient : IWebClient
    {
        private ILogger<NetHttpClient> _logger;

        public NetHttpClient()
        {
        }

        public NetHttpClient(ILogger<NetHttpClient> logger)
        {
            _logger = logger;
        }

        public string Load(Uri uri)
        {
            var httpClient = new HttpClient();
            var htmlPageBody = httpClient.GetStringAsync(uri).GetAwaiter().GetResult();

            CheckResult(htmlPageBody, uri);

            return htmlPageBody;
        }

        public async Task<string> LoadAsync(Uri uri)
        {
            var httpClient = new HttpClient();
            var htmlPageBody = await httpClient.GetStringAsync(uri);

            CheckResult(htmlPageBody, uri);

            return htmlPageBody;
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
