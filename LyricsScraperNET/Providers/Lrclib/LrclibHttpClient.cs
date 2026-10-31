using LyricsScraperNET.Common;
using LyricsScraperNET.Helpers;
using LyricsScraperNET.Network;
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
    /// Unlike <see cref="NetHttpClient"/>, returns the body of a 404 response: it is JSON with TrackNotFound and is handled by the provider.
    /// </summary>
    internal sealed class LrclibHttpClient : IWebClient
    {
        private readonly ILogger<LrclibHttpClient>? _logger;
        private const int MaxAttempts = 2;
        private static readonly TimeSpan MaxRetryAfter = TimeSpan.FromSeconds(30);

        public LrclibHttpClient()
            : this(SharedHttpClient.Instance)
        {
        }

        public LrclibHttpClient(ILogger<LrclibHttpClient> logger)
            : this(SharedHttpClient.Instance, logger)
        {
        }

        /// <param name="httpClient">Not disposed by this class: the owner of the client keeps it alive while it is used.</param>
        /// <param name="logger">Optional logger for failed requests.</param>
        public LrclibHttpClient(HttpClient httpClient, ILogger<LrclibHttpClient>? logger = null)
        {
            Ensure.ArgumentNotNull(httpClient, nameof(httpClient));
            HttpClient = httpClient;
            _logger = logger;
        }

        internal HttpClient HttpClient { get; }

        public string Load(Uri uri, CancellationToken cancellationToken = default)
        {
            return LoadAsync(uri, cancellationToken).GetAwaiter().GetResult();
        }

        public async Task<string> LoadAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            // HttpClient does not check the token before passing the request to the handler.
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                for (int attempt = 1; attempt <= MaxAttempts; attempt++)
                {
                    using var request = CreateRequest(uri);
                    using var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

                    if (response.StatusCode == HttpStatusCode.TooManyRequests && attempt < MaxAttempts)
                    {
                        var delay = GetRetryAfterDelay(response);
                        _logger?.LogInformation("Lrclib rate limited for {Uri}. Waiting {Delay} before retry.", uri, delay);
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    if (response.StatusCode != HttpStatusCode.NotFound && !response.IsSuccessStatusCode)
                    {
                        _logger?.LogWarning("Lrclib request to {Uri} returned status code {StatusCode}.", uri, (int)response.StatusCode);
                        return string.Empty;
                    }

                    var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    if (string.IsNullOrWhiteSpace(content))
                        _logger?.LogDebug("Lrclib returned an empty body for {Uri}.", uri);

                    return content;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException ex)
            {
                // Not canceled by the caller: HttpClient.Timeout elapsed.
                _logger?.LogWarning(ex, "Lrclib request to {Uri} timed out.", uri);
                return string.Empty;
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogWarning(ex, "Lrclib request to {Uri} failed.", uri);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error while loading Lrclib {Uri}.", uri);
                return string.Empty;
            }

            return string.Empty;
        }

        private HttpRequestMessage CreateRequest(Uri uri)
        {
            var request = SharedHttpClient.CreateGetRequest(HttpClient, uri);
            // LRCLIB asks clients to identify themselves; X-User-Agent stays even if the consumer replaces User-Agent.
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
