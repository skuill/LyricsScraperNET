using LyricsScraperNET.Network;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Genius;
using LyricsScraperNET.Providers.KPopLyrics;
using LyricsScraperNET.Providers.Lrclib;
using LyricsScraperNET.Providers.LyricsFreak;
using LyricsScraperNET.Providers.SongLyrics;
using System;
using System.Net;
using System.Net.Http;
using Xunit;

namespace LyricsScraperNET.UnitTest.Network
{
    public class SharedHttpClientTests
    {
        [Fact]
        public void CreateHandler_RecyclesConnectionsAndDecompresses()
        {
            // Act
            using var handler = SharedHttpClient.CreateHandler();

            // Assert
            Assert.Equal(TimeSpan.FromMinutes(2), handler.PooledConnectionLifetime);
            Assert.Equal(DecompressionMethods.All, handler.AutomaticDecompression);
        }

        [Fact]
        public void Instance_IsCreatedOnce()
        {
            // Act & Assert
            Assert.Same(SharedHttpClient.Instance, SharedHttpClient.Instance);
        }

        /// <summary>
        /// A new HttpClient per provider instance would open a new connection pool every time a provider is created.
        /// </summary>
        [Theory]
        [MemberData(nameof(DefaultProviders))]
        public void ProviderConstructor_Default_UsesSharedHttpClient(Func<ExternalProviderBase> createProvider, Type expectedWebClientType)
        {
            // Act
            var first = createProvider();
            var second = createProvider();

            // Assert
            Assert.IsType(expectedWebClientType, first.WebClient);
            Assert.Same(SharedHttpClient.Instance, GetHttpClient(first.WebClient));
            Assert.Same(SharedHttpClient.Instance, GetHttpClient(second.WebClient));
        }

        public static TheoryData<Func<ExternalProviderBase>, Type> DefaultProviders() => new()
        {
            { () => new AZLyricsProvider(), typeof(NetHttpClient) },
            { () => new GeniusProvider(), typeof(NetHttpClient) },
            { () => new KPopLyricsProvider(), typeof(NetHttpClient) },
            { () => new LyricsFreakProvider(), typeof(NetHttpClient) },
            { () => new SongLyricsProvider(), typeof(NetHttpClient) },
            { () => new LrclibProvider(), typeof(LrclibHttpClient) },
        };

        /// <summary>
        /// Only LyricsFreak waits and retries 429 (#62), with its own client and with the consumer's one.
        /// LRCLIB has its own retrying client.
        /// </summary>
        [Theory]
        [MemberData(nameof(TooManyRequestsRetryDelays))]
        public void Provider_TooManyRequestsRetryDelay_SetOnlyForLyricsFreak(Func<ExternalProviderBase> createProvider, TimeSpan? expectedDelay)
        {
            // Arrange
            var provider = createProvider();
            using var httpClient = new HttpClient();

            // Act
            var defaultDelay = Assert.IsType<NetHttpClient>(provider.WebClient).TooManyRequestsRetryDelay;
            provider.WithHttpClient(httpClient);
            var webClient = Assert.IsType<NetHttpClient>(provider.WebClient);

            // Assert
            Assert.Equal(expectedDelay, defaultDelay);
            Assert.Equal(expectedDelay, webClient.TooManyRequestsRetryDelay);
            Assert.Same(httpClient, webClient.HttpClient);
        }

        public static TheoryData<Func<ExternalProviderBase>, TimeSpan?> TooManyRequestsRetryDelays() => new()
        {
            { () => new AZLyricsProvider(), null },
            { () => new GeniusProvider(), null },
            { () => new KPopLyricsProvider(), null },
            { () => new LyricsFreakProvider(), TimeSpan.FromSeconds(2) },
            { () => new SongLyricsProvider(), null },
        };

        [Theory]
        // No Retry-After or zero: the fallback.
        [InlineData(null, 2, 2)]
        [InlineData(0, 2, 2)]
        [InlineData(5, 2, 5)]
        // At most 30 s.
        [InlineData(120, 2, 30)]
        public void GetRetryAfterDelay_MultipleInputs(int? retryAfterSeconds, int fallbackSeconds, int expectedSeconds)
        {
            // Arrange
            using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
            if (retryAfterSeconds != null)
                response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(retryAfterSeconds.Value));

            // Act
            var actual = SharedHttpClient.GetRetryAfterDelay(response, TimeSpan.FromSeconds(fallbackSeconds));

            // Assert
            Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), actual);
        }

        [Fact]
        public void GetRetryAfterDelay_RetryAfterDate_ReturnsFallback()
        {
            // Arrange
            using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
            response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(DateTimeOffset.UtcNow.AddSeconds(10));

            // Act & Assert
            Assert.Equal(TimeSpan.FromSeconds(2), SharedHttpClient.GetRetryAfterDelay(response, TimeSpan.FromSeconds(2)));
        }

        internal static HttpClient GetHttpClient(IWebClient webClient) => webClient switch
        {
            NetHttpClient netHttpClient => netHttpClient.HttpClient,
            LrclibHttpClient lrclibHttpClient => lrclibHttpClient.HttpClient,
            _ => throw new InvalidOperationException($"Unexpected web client {webClient.GetType().Name}"),
        };
    }
}
