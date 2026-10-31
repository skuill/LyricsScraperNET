using LyricsScraperNET.Common;
using System;
using System.Net;
using System.Net.Http;

namespace LyricsScraperNET.Network
{
    /// <summary>
    /// The <see cref="HttpClient"/> used by all providers unless the consumer sets its own one.
    /// Follows the .NET guidelines for long-lived clients: https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient-guidelines#recommended-use
    /// </summary>
    internal static class SharedHttpClient
    {
        /// <summary>
        /// Connections are recreated after this interval, so DNS changes are picked up in long-running apps.
        /// </summary>
        internal static readonly TimeSpan PooledConnectionLifetime = TimeSpan.FromMinutes(2);

        /// <summary>
        /// One instance (and one connection pool) for the whole library: creating a client per request or per provider
        /// leaves sockets in TIME_WAIT and can exhaust the ports.
        /// </summary>
        internal static HttpClient Instance { get; } = new HttpClient(CreateHandler(), disposeHandler: true);

        internal static SocketsHttpHandler CreateHandler()
        {
            var handler = new SocketsHttpHandler();
            ConfigureHandler(handler);
            return handler;
        }

        /// <summary>
        /// Also applied to the handler of the DI client, so both setups behave the same.
        /// </summary>
        internal static void ConfigureHandler(SocketsHttpHandler handler)
        {
            handler.PooledConnectionLifetime = PooledConnectionLifetime;
            handler.AutomaticDecompression = DecompressionMethods.All;
        }

        /// <summary>
        /// Creates a GET request. The library User-Agent is added only if the client has no own one,
        /// so a consumer can replace it in <see cref="HttpClient.DefaultRequestHeaders"/>.
        /// </summary>
        internal static HttpRequestMessage CreateGetRequest(HttpClient httpClient, Uri uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            // NonValidated: the default headers are read while other threads send requests through the same client.
            if (!httpClient.DefaultRequestHeaders.NonValidated.Contains(UserAgentHeaderName))
                request.Headers.TryAddWithoutValidation(UserAgentHeaderName, Constants.LibraryUserAgent);

            return request;
        }

        private const string UserAgentHeaderName = "User-Agent";
    }
}
