using LyricsScraperNET.Common;
using LyricsScraperNET.Helpers;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Network.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace LyricsScraperNET.Network
{
    /// <summary>
    /// Provides an implementation of the <see cref="IWebClient"/> interface using <see cref="System.Net.Http.HttpClient"/>.
    /// Uses <see cref="SharedHttpClient.Instance"/> unless another client is passed.
    /// </summary>
    internal sealed class NetHttpClient : IWebClientWithStatus
    {
        private readonly ILogger<NetHttpClient>? _logger;

        public NetHttpClient()
            : this(SharedHttpClient.Instance)
        {
        }

        public NetHttpClient(ILogger<NetHttpClient> logger)
            : this(SharedHttpClient.Instance, logger)
        {
        }

        /// <param name="httpClient">Not disposed by this class: the owner of the client keeps it alive while it is used.</param>
        /// <param name="logger">Optional logger for failed requests.</param>
        /// <param name="configureRequest">Optional hook called for every request before it is sent, for example to add headers.</param>
        /// <param name="tooManyRequestsRetryDelay">If set, a 429 Too Many Requests response is retried once after Retry-After,
        /// or after this delay if the site does not send it. Not set: 429 is a failed load.</param>
        public NetHttpClient(HttpClient httpClient, ILogger<NetHttpClient>? logger = null, Action<HttpRequestMessage>? configureRequest = null, TimeSpan? tooManyRequestsRetryDelay = null)
        {
            Ensure.ArgumentNotNull(httpClient, nameof(httpClient));
            HttpClient = httpClient;
            _logger = logger;
            ConfigureRequest = configureRequest;
            TooManyRequestsRetryDelay = tooManyRequestsRetryDelay;
        }

        internal HttpClient HttpClient { get; }

        internal Action<HttpRequestMessage>? ConfigureRequest { get; }

        internal TimeSpan? TooManyRequestsRetryDelay { get; }

        /// <inheritdoc />
        [Obsolete(Obsoletions.SyncApi.LoadMessage, DiagnosticId = Obsoletions.SyncApi.DiagnosticId, UrlFormat = Obsoletions.SharedUrlFormat)]
        public string Load(Uri uri, CancellationToken cancellationToken = default)
        {
            return LoadAsync(uri, cancellationToken).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public async Task<string> LoadAsync(Uri uri, CancellationToken cancellationToken = default)
            => (await LoadWithStatusAsync(uri, cancellationToken).ConfigureAwait(false)).Body;

        /// <inheritdoc />
        public async Task<WebLoadResult> LoadWithStatusAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            // HttpClient does not check the token before passing the request to the handler.
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var response = await SendAsync(uri, cancellationToken).ConfigureAwait(false);

                var statusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    _logger?.LogWarning("Request to {Uri} returned status code {StatusCode}.", uri, statusCode);
                    return new WebLoadResult(string.Empty, response.StatusCode == HttpStatusCode.NotFound ? WebLoadStatus.NotFound : WebLoadStatus.Failed, statusCode);
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(content))
                    _logger?.LogDebug("Request to {Uri} returned an empty body.", uri);

                return new WebLoadResult(content, WebLoadStatus.Success, statusCode);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException ex)
            {
                // Not canceled by the caller: HttpClient.Timeout elapsed.
                _logger?.LogWarning(ex, "Request to {Uri} timed out.", uri);
                return new WebLoadResult(string.Empty, WebLoadStatus.Failed, null);
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogWarning(ex, "Request to {Uri} failed.", uri);
                return new WebLoadResult(string.Empty, WebLoadStatus.Failed, null);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error while loading {Uri}.", uri);
                return new WebLoadResult(string.Empty, WebLoadStatus.Failed, null);
            }
        }

        private async Task<HttpResponseMessage> SendAsync(Uri uri, CancellationToken cancellationToken)
        {
            var response = await SendOnceAsync(uri, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.TooManyRequests || TooManyRequestsRetryDelay is not { } fallbackDelay)
                return response;

            var delay = SharedHttpClient.GetRetryAfterDelay(response, fallbackDelay);
            response.Dispose();
            _logger?.LogInformation("Request to {Uri} returned status code 429. Waiting {Delay} before retry.", uri, delay);
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

            return await SendOnceAsync(uri, cancellationToken).ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> SendOnceAsync(Uri uri, CancellationToken cancellationToken)
        {
            using var request = SharedHttpClient.CreateGetRequest(HttpClient, uri);
            ConfigureRequest?.Invoke(request);
            return await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        }
    }
}
