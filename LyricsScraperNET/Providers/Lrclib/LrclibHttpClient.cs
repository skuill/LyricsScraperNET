using LyricsScraperNET.Common;
using LyricsScraperNET.Network.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.Lrclib
{
    /// <summary>
    /// HTTP client for LRCLIB. Identifies the library via User-Agent and honors Retry-After on 429.
    /// </summary>
    internal sealed class LrclibHttpClient : IWebClient
    {
        private readonly ILogger<LrclibHttpClient>? _logger;
        private static readonly HttpClient _httpClient = new();
        private const int MaxAttempts = 2;
        private static readonly TimeSpan MaxRetryAfter = TimeSpan.FromSeconds(30);

        public LrclibHttpClient()
        {
        }

        public LrclibHttpClient(ILogger<LrclibHttpClient> logger) : this()
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
                _logger?.LogWarning($"Lrclib HTTP request failed for uri: {uri}. Exception: {ex}");
                return string.Empty;
            }
        }

        public async Task<string> LoadAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            try
            {
                for (int attempt = 1; attempt <= MaxAttempts; attempt++)
                {
                    using var request = CreateRequest(uri);
                    var response = await _httpClient.SendAsync(request, cancellationToken);

                    if (response.StatusCode == HttpStatusCode.TooManyRequests && attempt < MaxAttempts)
                    {
                        var delay = GetRetryAfterDelay(response);
                        _logger?.LogInformation($"Lrclib rate limited for uri: {uri}. Waiting {delay.TotalSeconds}s before retry.");
                        await Task.Delay(delay, cancellationToken);
                        continue;
                    }

                    // 404 body is JSON with TrackNotFound and is handled by the provider.
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return await response.Content.ReadAsStringAsync(cancellationToken);
                    }

                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        _logger?.LogDebug($"Lrclib returned empty content for uri: {uri}");
                    }

                    return content;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogWarning($"Lrclib HTTP request failed for URI: {uri}. Exception: {ex}");
                return string.Empty;
            }
            catch (OperationCanceledException ex)
            {
                _logger?.LogInformation($"Lrclib request for URI: {uri} was canceled. Exception: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"An unexpected error occurred while loading Lrclib URI: {uri}. Exception: {ex}");
                return string.Empty;
            }

            return string.Empty;
        }

        private static HttpRequestMessage CreateRequest(Uri uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.TryAddWithoutValidation("User-Agent", Constants.LibraryUserAgent);
            request.Headers.TryAddWithoutValidation("X-User-Agent", Constants.LibraryUserAgent);
            return request;
        }

        private static TimeSpan GetRetryAfterDelay(HttpResponseMessage response)
        {
            var retryAfter = response.Headers.RetryAfter?.Delta;
            if (retryAfter == null || retryAfter.Value <= TimeSpan.Zero)
                return TimeSpan.FromSeconds(1);

            return retryAfter.Value > MaxRetryAfter ? MaxRetryAfter : retryAfter.Value;
        }
    }
}
