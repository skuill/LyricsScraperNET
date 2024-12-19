using LyricsScraperNET.Network.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.Network
{
    internal sealed class NetHttpClient : IWebClient
    {
        private readonly ILogger<NetHttpClient> _logger;
        private static HttpClient _httpClient = new HttpClient();

        public NetHttpClient()
        {
        }

        public NetHttpClient(ILogger<NetHttpClient> logger) : this()
        {
            _logger = logger;
        }

        public string Load(Uri uri, CancellationToken cancellationToken = default)
        {
            try
            {
                return LoadAsync(uri, cancellationToken).GetAwaiter().GetResult();
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogWarning($"HttpClient GetStringAsync throw exception for uri: {uri}. Exception: {ex}");
                return string.Empty;
            }
        }

        public async Task<string> LoadAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            string htmlPageBody;

            try
            {
#if NETSTANDARD
                htmlPageBody = await _httpClient.GetStringAsync(uri);
#else
                htmlPageBody = await _httpClient.GetStringAsync(uri, cancellationToken);
#endif
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogWarning($"HttpClient GetStringAsync threw exception for URI: {uri}. Exception: {ex}");
                return string.Empty;
            }
            catch (OperationCanceledException ex)
            {
                _logger?.LogInformation($"Load request for URI: {uri} was canceled. Exception: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"An unexpected error occurred while loading URI: {uri}. Exception: {ex}");
                return string.Empty;
            }

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
