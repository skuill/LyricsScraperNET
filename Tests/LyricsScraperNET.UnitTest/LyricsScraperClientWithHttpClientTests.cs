using FakeItEasy;
using LyricsScraperNET.Common;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Network;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Lrclib;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.UnitTest.Network;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using static LyricsScraperNET.UnitTest.Network.HttpTestHelper;

namespace LyricsScraperNET.UnitTest
{
    public class LyricsScraperClientWithHttpClientTests
    {
        [Fact]
        public void WithHttpClient_AppliesToProvidersAddedBeforeAndAfter()
        {
            // Arrange
            using var httpClient = new HttpClient();
            var client = new LyricsScraperClient();
            var addedBefore = new AZLyricsProvider();
            var addedAfter = new LrclibProvider();
            client.AddProvider(addedBefore);

            // Act
            var returned = client.WithHttpClient(httpClient);
            client.AddProvider(addedAfter);

            // Assert
            Assert.Same(client, returned);
            Assert.IsType<NetHttpClient>(addedBefore.WebClient);
            Assert.Same(httpClient, SharedHttpClientTests.GetHttpClient(addedBefore.WebClient));
            Assert.IsType<LrclibHttpClient>(addedAfter.WebClient);
            Assert.Same(httpClient, SharedHttpClientTests.GetHttpClient(addedAfter.WebClient));
        }

        [Fact]
        public async Task WithHttpClient_FluentSetupWithAllProviders_SearchGoesThroughTheGivenClient()
        {
            // Arrange
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, HttpStatusCode.NotFound, "<html>not found</html>");
            var client = new LyricsScraperClient()
                .WithHttpClient(httpClient)
                .WithAllProviders();
            var expectedUri = new AZLyricsUriConverter().GetLyricUri("Artist", "Song");

            // Act
            var searchResult = await client.SearchLyricAsync(
                new ArtistAndSongSearchRequest("Artist", "Song", ExternalProviderType.AZLyrics), TestContext.Current.CancellationToken);

            // Assert
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(new[] { new SentRequest(HttpMethod.Get, expectedUri, Constants.LibraryUserAgent, null) }, Snapshot(sentRequests));
            CallToSendAsync(handler).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void WithHttpClient_ProviderNotBasedOnExternalProviderBase_IsLeftUntouched()
        {
            // Arrange
            using var httpClient = new HttpClient();
            var client = new LyricsScraperClient();
            var customBefore = A.Fake<IExternalProvider>();
            var customAfter = A.Fake<IExternalProvider>();
            client.AddProvider(customBefore);

            // Act
            var exception = Record.Exception(() =>
            {
                client.WithHttpClient(httpClient);
                client.AddProvider(customAfter);
            });

            // Assert
            Assert.Null(exception);
            A.CallTo(() => customBefore.WithWebClient(A<IWebClient>._)).MustNotHaveHappened();
            A.CallTo(() => customAfter.WithWebClient(A<IWebClient>._)).MustNotHaveHappened();
        }

        [Fact]
        public void WithHttpClient_Null_ThrowsArgumentNullException()
        {
            // Arrange
            var client = new LyricsScraperClient();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => client.WithHttpClient(null));
        }
    }
}
