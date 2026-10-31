using LyricsScraperNET.Helpers;
using LyricsScraperNET.Network.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.Network
{
    /// <summary>
    /// Provides an implementation of the <see cref="IWebClient"/> interface using <see cref="System.Net.Http.HttpClient"/>.
    /// Uses <see cref="SharedHttpClient.Instance"/> unless another client is passed.
    /// </summary>
    internal sealed class NetHttpClient : IWebClient
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
        public NetHttpClient(HttpClient httpClient, ILogger<NetHttpClient>? logger = null)
        {
            Ensure.ArgumentNotNull(httpClient, nameof(httpClient));
            HttpClient = httpClient;
            _logger = logger;
        }

        internal HttpClient HttpClient { get; }

        /// <inheritdoc />
        public string Load(Uri uri, CancellationToken cancellationToken = default)
        {
            return LoadAsync(uri, cancellationToken).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public async Task<string> LoadAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            // HttpClient does not check the token before passing the request to the handler.
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var request = SharedHttpClient.CreateGetRequest(HttpClient, uri);
                using var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    _logger?.LogWarning("Request to {Uri} returned status code {StatusCode}.", uri, (int)response.StatusCode);
                    return string.Empty;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(content))
                    _logger?.LogDebug("Request to {Uri} returned an empty body.", uri);

                return content;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException ex)
            {
                // Not canceled by the caller: HttpClient.Timeout elapsed.
                _logger?.LogWarning(ex, "Request to {Uri} timed out.", uri);
                return string.Empty;
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogWarning(ex, "Request to {Uri} failed.", uri);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error while loading {Uri}.", uri);
                return string.Empty;
            }
        }
    }
}
