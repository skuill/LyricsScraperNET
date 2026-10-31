using FakeItEasy;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Network.Models;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Genius;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.Providers.SongLyrics;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static LyricsScraperNET.UnitTest.Network.HttpTestHelper;

namespace LyricsScraperNET.UnitTest.Providers
{
    /// <summary>
    /// The base class remembers the last page load of each search call, so a failed site is not reported as "not found" (#74).
    /// </summary>
    public class ExternalProviderBaseLoadTrackingTests
    {
        private static readonly Uri FailingUri = new("https://lyrics.example/fail");
        private static readonly Uri OkUri = new("https://lyrics.example/ok");

        [Fact]
        public async Task SearchLyricAsync_ConcurrentSearchesOnOneProvider_EachGetsItsOwnLoadOutcome()
        {
            // Arrange: both requests are in flight at the same time before any of them answers.
            var bothStarted = new CountdownEvent(2);
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var (httpClient, _) = CreateHttpClient(new List<SentRequest>(), async (request, _) =>
            {
                bothStarted.Signal();
                await release.Task;
                return request.RequestUri.AbsoluteUri.Contains("failing-artist")
                    ? CreateResponse(HttpStatusCode.ServiceUnavailable, "busy")
                    : CreateResponse(HttpStatusCode.OK, "<html>no lyrics container</html>");
            });
            var provider = new SongLyricsProvider();
            provider.WithHttpClient(httpClient);
            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            var failingSearch = Task.Run(() => provider.SearchLyricAsync(new ArtistAndSongSearchRequest("failing-artist", "song"), cancellationToken), cancellationToken);
            var answeringSearch = Task.Run(() => provider.SearchLyricAsync(new ArtistAndSongSearchRequest("answering-artist", "song"), cancellationToken), cancellationToken);
            Assert.True(bothStarted.Wait(SafetyTimeout, cancellationToken));
            release.SetResult();
            var results = await Task.WhenAll(failingSearch, answeringSearch).WaitAsync(SafetyTimeout, cancellationToken);

            // Assert
            Assert.Equal(new WebLoadResult(string.Empty, WebLoadStatus.Failed, 503), results[0].LoadFailure);
            Assert.Null(results[1].LoadFailure);
            Assert.All(results, result => Assert.Equal(SearchResultCode.NotFound, result.Code));
        }

        [Theory]
        [InlineData(true, false, false)]
        [InlineData(false, true, true)]
        [InlineData(true, true, true)]
        public async Task SearchLyricAsync_TwoLoads_TheLastLoadDecides(bool firstFails, bool secondFails, bool expectFailure)
        {
            // Arrange
            var provider = new TwoLoadsProvider(firstFails ? FailingUri : OkUri, secondFails ? FailingUri : OkUri, found: false);
            provider.WithHttpClient(CreateFailingOrOkHttpClient());

            // Act
            var result = await provider.SearchLyricAsync(new ArtistAndSongSearchRequest("Artist", "Song"), TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(expectFailure, result.LoadFailure != null);
            Assert.Equal(SearchResultCode.NotFound, result.Code);
        }

        [Fact]
        public async Task SearchLyricAsync_FoundAfterFailedLoad_NoLoadFailure()
        {
            // Arrange
            var provider = new TwoLoadsProvider(OkUri, FailingUri, found: true);
            provider.WithHttpClient(CreateFailingOrOkHttpClient());

            // Act
            var result = await provider.SearchLyricAsync(new ArtistAndSongSearchRequest("Artist", "Song"), TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(SearchResultCode.Found, result.Code);
            Assert.Null(result.LoadFailure);
        }

        [Fact]
        public async Task SearchLyricAsync_OwnWebClientWithoutStatus_NoLoadFailure()
        {
            // Arrange: an IWebClient of the consumer cannot tell a failure from an empty page.
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).Returns(string.Empty);
            var provider = new AZLyricsProvider();
            provider.WithWebClient(webClient);

            // Act
            var result = await provider.SearchLyricAsync(new ArtistAndSongSearchRequest("Artist", "Song"), TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(SearchResultCode.NotFound, result.Code);
            Assert.Null(result.LoadFailure);
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SearchLyricAsync_NestedSyncSearchOfAnotherProvider_KeepsTheOuterLoadOutcome()
        {
            // Arrange: the inner sync search must restore the tracker of the outer call.
            var inner = new TwoLoadsProvider(OkUri, OkUri, found: false);
            inner.WithHttpClient(CreateFailingOrOkHttpClient());
            var outer = new TwoLoadsProvider(FailingUri, FailingUri, found: false, beforeLoads: () => inner.SearchLyric(new ArtistAndSongSearchRequest("Inner", "Song"), TestContext.Current.CancellationToken));
            outer.WithHttpClient(CreateFailingOrOkHttpClient());

            // Act
            var result = await outer.SearchLyricAsync(new ArtistAndSongSearchRequest("Outer", "Song"), TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result.LoadFailure);
        }

        [Fact]
        public void SearchLyric_SyncSearch_TracksLoadsToo()
        {
            // Arrange
            var provider = new TwoLoadsProvider(OkUri, FailingUri, found: false);
            provider.WithHttpClient(CreateFailingOrOkHttpClient());

            // Act
            var result = provider.SearchLyric(new ArtistAndSongSearchRequest("Artist", "Song"), TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(new WebLoadResult(string.Empty, WebLoadStatus.Failed, 503), result.LoadFailure);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK, null)]
        [InlineData(HttpStatusCode.ServiceUnavailable, 503)]
        public async Task GeniusSearchLyricAsync_ApiKeyRejected_LastLoadOfThePublicSearchDecides(HttpStatusCode publicSearchStatus, int? expectedFailureStatus)
        {
            // Arrange: the API answers 401, then the public search either answers without a matching hit or fails.
            var searchUri = new GeniusUriConverter().GetLyricUri("Artist", "Song");
            var (httpClient, _) = CreateHttpClient(new List<SentRequest>(), (request, _) => Task.FromResult(
                request.RequestUri == searchUri
                    ? CreateResponse(publicSearchStatus, "{\"response\":{\"hits\":[]}}")
                    : CreateResponse(HttpStatusCode.Unauthorized, "{\"error\":\"invalid_token\"}")));
            var provider = new GeniusProvider(new GeniusOptions { Enabled = true, ApiKey = "invalid" });
            provider.WithHttpClient(httpClient);

            // Act
            var result = await provider.SearchLyricAsync(new ArtistAndSongSearchRequest("Artist", "Song"), TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(SearchResultCode.NotFound, result.Code);
            Assert.Equal(expectedFailureStatus, result.LoadFailure?.StatusCode);
        }

        private static readonly TimeSpan SafetyTimeout = TimeSpan.FromSeconds(10);

        private static HttpClient CreateFailingOrOkHttpClient()
            => CreateHttpClient(new List<SentRequest>(), (request, _) => Task.FromResult(
                request.RequestUri == FailingUri
                    ? CreateResponse(HttpStatusCode.ServiceUnavailable, "busy")
                    : CreateResponse(HttpStatusCode.OK, "page"))).HttpClient;

        /// <summary>
        /// Loads two pages through the base class and returns "found" or "not found".
        /// </summary>
        private sealed class TwoLoadsProvider : ExternalProviderBase
        {
            private readonly Uri _first;
            private readonly Uri _second;
            private readonly bool _found;
            private readonly Action _beforeLoads;

            public TwoLoadsProvider(Uri first, Uri second, bool found, Action beforeLoads = null)
            {
                _first = first;
                _second = second;
                _found = found;
                _beforeLoads = beforeLoads;
            }

            public override IExternalProviderOptions Options { get; } = new AZLyricsOptions { Enabled = true };

            protected override async Task<SearchResult> SearchLyricAsync(string artist, string song, CancellationToken cancellationToken = default)
            {
                _beforeLoads?.Invoke();
                await LoadPageAsync(_first, cancellationToken);
                await LoadPageAsync(_second, cancellationToken);
                return _found ? new SearchResult("Lyrics", ExternalProviderType.AZLyrics) : new SearchResult(ExternalProviderType.AZLyrics);
            }
        }
    }
}
