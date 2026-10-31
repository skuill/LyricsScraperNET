using FakeItEasy;
using LyricsScraperNET.Common;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Network;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Genius;
using LyricsScraperNET.Providers.KPopLyrics;
using LyricsScraperNET.Providers.Lrclib;
using LyricsScraperNET.Providers.LyricsFreak;
using LyricsScraperNET.Providers.SongLyrics;
using LyricsScraperNET.UnitTest.Network;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using static LyricsScraperNET.UnitTest.Network.HttpTestHelper;

namespace LyricsScraperNET.UnitTest.Providers
{
    public class ExternalProviderWithHttpClientTests
    {
        [Theory]
        [MemberData(nameof(Providers))]
        public void WithHttpClient_ReplacesWebClientWithOneOverTheGivenClient(Func<ExternalProviderBase> createProvider, Type expectedWebClientType)
        {
            // Arrange
            var provider = createProvider();
            using var httpClient = new HttpClient();

            // Act
            provider.WithHttpClient(httpClient);

            // Assert
            Assert.IsType(expectedWebClientType, provider.WebClient);
            Assert.Same(httpClient, SharedHttpClientTests.GetHttpClient(provider.WebClient));
        }

        public static TheoryData<Func<ExternalProviderBase>, Type> Providers() => SharedHttpClientTests.DefaultProviders();

        [Fact]
        public void WithHttpClient_Null_ThrowsAndKeepsWebClient()
        {
            // Arrange
            var provider = new AZLyricsProvider();
            var webClient = provider.WebClient;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => provider.WithHttpClient(null!));
            Assert.Same(webClient, provider.WebClient);
        }

        [Fact]
        public async Task SearchLyricAsync_AfterWithHttpClient_SendsRequestThroughTheGivenClient()
        {
            // Arrange
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, HttpStatusCode.NotFound, "<html>not found</html>");
            var provider = new AZLyricsProvider();
            provider.WithHttpClient(httpClient);
            var expectedUri = new AZLyricsUriConverter().GetLyricUri("Artist", "Song");

            // Act
            var searchResult = await provider.SearchLyricAsync(new ArtistAndSongSearchRequest("Artist", "Song"), TestContext.Current.CancellationToken);

            // Assert
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(new[] { new SentRequest(HttpMethod.Get, expectedUri, Constants.LibraryUserAgent, null) }, Snapshot(sentRequests));
            CallToSendAsync(handler).MustHaveHappenedOnceExactly();
        }
    }
}
