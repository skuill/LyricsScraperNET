using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Providers.KPopLyrics;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.TestShared.Extensions;
using LyricsScraperNET.TestShared.Providers;
using LyricsScraperNET.TestShared.TestModel;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.KPopLyrics
{
    public class KPopLyricsProviderTest : ProviderTestBase
    {
        [Theory]
        [MemberData(nameof(GetTestData), arguments: "Providers\\KPopLyrics\\lyric_test_data.json")]
        public void SearchLyric_UnitDynamicData_Success(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new KPopLyricsProvider();
            lyricsClient.ConfigureExternalProviderWithArtist(testData);

            var searchRequest = CreateSearchRequest(testData);
            var cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.False(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.KPopLyrics, searchResult.ExternalProviderType);
            Assert.Equal(testData.LyricResultData.Replace("\r", string.Empty), searchResult.LyricText);
        }

        // Synthetic responses with only the data the provider depends on (#69).
        // The remix comes first: the site used to redirect a guessed URL to it.
        private const string SearchJson = """
            [
              {"title":{"rendered":"Test Artist &#8211; Test Song (Holiday Remix) Lyrics"},"link":"https:\/\/www.kpoplyrics.net\/test-artist-test-song-holiday-remix-lyrics-english-romanized.html"},
              {"title":{"rendered":"Other Artist &#8211; Test Song Lyrics"},"link":"https:\/\/www.kpoplyrics.net\/other-artist-test-song-lyrics-english-romanized.html"},
              {"title":{"rendered":"Test Artist &#8211; Test Song (Video)"},"link":"https:\/\/www.kpoplyrics.net\/test-artist-test-song-video.html"},
              {"title":{"rendered":"Test Artist &#8211; Test Song Lyrics"},"link":"https:\/\/www.kpoplyrics.net\/test-artist-test-song-lyrics-english-romanized.html"},
              {"title":{"rendered":"Test Artist &#8211; Don&#8217;t Stop Lyrics"},"link":"https:\/\/www.kpoplyrics.net\/test-artist-dont-stop-lyrics.html"},
              {"title":{"rendered":"&amp;TEST &#8211; Song Lyrics"},"link":"https:\/\/www.kpoplyrics.net\/test-song-lyrics-english-romanized.html"},
              {"title":{"rendered":"Test Artist &#8211; Other Site Lyrics"},"link":"https:\/\/www.example.com\/test-artist-other-site-lyrics.html"},
              {"title":{"rendered":"Test Artist &#8211; Plain Http Lyrics"},"link":"http:\/\/www.kpoplyrics.net\/test-artist-plain-http-lyrics.html"},
              {"title":{"rendered":"Test Artist &#8211; No Link Lyrics"}},
              {"link":"https:\/\/www.kpoplyrics.net\/no-title.html"}
            ]
            """;
        private const string LyricPage = "<html><body><div class='entry-content mh-clearfix'><h2>Test Song Romanized Lyrics</h2><p>Test line one<br> Test line two</p></div></body></html>";

        [Theory]
        [InlineData("Test Artist", "Test Song", "test-artist-test-song-lyrics-english-romanized.html")]
        [InlineData("TEST ARTIST", "test song", "test-artist-test-song-lyrics-english-romanized.html")]
        [InlineData("Test Artist (테스트)", "Test Song (테스트 송)", "test-artist-test-song-lyrics-english-romanized.html")]
        [InlineData("Test Artist", "Test Song (Holiday Remix)", "test-artist-test-song-holiday-remix-lyrics-english-romanized.html")]
        [InlineData("Test Artist", "Dont Stop", "test-artist-dont-stop-lyrics.html")]
        [InlineData("&TEST", "Song", "test-song-lyrics-english-romanized.html")]
        public async Task SearchLyricAsync_PostWithSameArtistAndSong_LoadsItsPage(string artist, string song, string expectedPage)
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var searchUri = new KPopLyricsUriConverter().GetLyricUri(artist, song);
            var songUri = new Uri("https://www.kpoplyrics.net/" + expectedPage);
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(searchUri, cancellationToken)).Returns(SearchJson);
            A.CallTo(() => webClient.LoadAsync(songUri, cancellationToken)).Returns(LyricPage);

            var lyricsClient = new KPopLyricsProvider();
            lyricsClient.WithWebClient(webClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest(artist, song), cancellationToken);

            // Assert
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.Equal(ExternalProviderType.KPopLyrics, searchResult.ExternalProviderType);
            Assert.Equal("Test line one\nTest line two", searchResult.LyricText);
            A.CallTo(() => webClient.LoadAsync(searchUri, cancellationToken)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => webClient.LoadAsync(songUri, cancellationToken)).MustHaveHappenedOnceExactly());
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustHaveHappenedTwiceExactly();
        }

        [Theory]
        // A part of the title is not enough, and another version is another song.
        [InlineData("Test Artist", "Song")]
        [InlineData("Test Artist", "Test Song (Live)")]
        [InlineData("Another Artist", "Test Song")]
        // Links outside the site, plain HTTP or no link at all are not loaded.
        [InlineData("Test Artist", "Other Site")]
        [InlineData("Test Artist", "Plain Http")]
        [InlineData("Test Artist", "No Link")]
        public async Task SearchLyricAsync_NoPostToLoad_ReturnsNoDataFoundWithoutLoadingPage(string artist, string song)
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).Returns(SearchJson);

            var lyricsClient = new KPopLyricsProvider();
            lyricsClient.WithWebClient(webClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest(artist, song), cancellationToken);

            // Assert
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.Equal(ExternalProviderType.KPopLyrics, searchResult.ExternalProviderType);
            A.CallTo(() => webClient.LoadAsync(new KPopLyricsUriConverter().GetLyricUri(artist, song), cancellationToken)).MustHaveHappenedOnceExactly();
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData("")]
        [InlineData("[]")]
        [InlineData("null")]
        [InlineData("<html><body>Not JSON</body></html>")]
        [InlineData("{\"code\":\"rest_no_route\",\"message\":\"No route was found\"}")]
        public async Task SearchLyricAsync_EmptyOrUnexpectedSearchResponse_ReturnsNoDataFound(string searchResponse)
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).Returns(searchResponse);

            var lyricsClient = new KPopLyricsProvider();
            lyricsClient.WithWebClient(webClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest("Test Artist", "Test Song"), cancellationToken);

            // Assert
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData("방탄소년단", "Dynamite")]
        [InlineData("BTS", "다이너마이트")]
        [InlineData("BTS", "(다이너마이트)")]
        public async Task SearchLyricAsync_NameWithoutLatinLetters_ReturnsNoDataFoundWithoutRequests(string artist, string song)
        {
            // Arrange
            var webClient = A.Fake<IWebClient>();
            var lyricsClient = new KPopLyricsProvider();
            lyricsClient.WithWebClient(webClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest(artist, song), TestContext.Current.CancellationToken);

            // Assert
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.Equal(ExternalProviderType.KPopLyrics, searchResult.ExternalProviderType);
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task SearchLyricAsync_ByUri_LoadsThePageWithoutSearch()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var uri = new Uri("https://www.kpoplyrics.net/test-artist-test-song-lyrics-english-romanized.html");
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(uri, cancellationToken)).Returns(LyricPage);

            var lyricsClient = new KPopLyricsProvider();
            lyricsClient.WithWebClient(webClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new UriSearchRequest(uri), cancellationToken);

            // Assert
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.Equal("Test line one\nTest line two", searchResult.LyricText);
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }
    }
}
