using LyricsScraperNET.Network.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.Network
{
    /// <summary>
    /// Provides an implementation of the <see cref="IWebClient"/> interface using <see cref="HttpClient"/> 
    /// to perform HTTP requests for loading web resources.
    /// This class is designed to handle network operations with proper cancellation support and logging.
    /// </summary>
    internal sealed class NetHttpClient : IWebClient
    {
        private readonly ILogger<NetHttpClient>? _logger;

        // HttpClient is declared as static to prevent frequent creation and disposal of instances, 
        // which can lead to socket exhaustion due to delays in releasing resources.
        // Using a single instance ensures efficient connection management 
        // and minimizes overhead for establishing new HTTP connections.
        // More details: https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines#static-or-instance
        private static HttpClient _httpClient = new HttpClient();

        public NetHttpClient()
        {
        }

        public NetHttpClient(ILogger<NetHttpClient> logger) : this()
        {
            _logger = logger;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public async Task<string> LoadAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            HttpResponseMessage response;

            try
            {
                response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
                //#if NETSTANDARD
                //                htmlPageBody = await _httpClient.GetStringAsync(uri);
                //#else
                //                htmlPageBody = await _httpClient.GetStringAsync(uri, cancellationToken);
                //#endif
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
