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

        internal static HttpClient GetHttpClient(IWebClient webClient) => webClient switch
        {
            NetHttpClient netHttpClient => netHttpClient.HttpClient,
            LrclibHttpClient lrclibHttpClient => lrclibHttpClient.HttpClient,
            _ => throw new InvalidOperationException($"Unexpected web client {webClient.GetType().Name}"),
        };
    }
}
