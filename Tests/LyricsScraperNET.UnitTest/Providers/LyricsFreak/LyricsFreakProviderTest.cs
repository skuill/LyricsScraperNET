using FakeItEasy;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Providers.LyricsFreak;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.TestShared.Extensions;
using LyricsScraperNET.TestShared.Providers;
using LyricsScraperNET.TestShared.TestModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.LyricsFreak
{
    public class LyricsFreakProviderTest : ProviderTestBase
    {
        [Theory]
        [MemberData(nameof(GetTestData), arguments: "Providers\\LyricsFreak\\lyric_test_data.json")]
        public void SearchLyric_UnitDynamicData_Success(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new LyricsFreakProvider();
            lyricsClient.ConfigureExternalProviderWithArtist(testData);

            SearchRequest searchRequest = CreateSearchRequest(testData);
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.LyricsFreak, searchResult.ExternalProviderType);
            Assert.Equal(testData.LyricResultData.Replace("\r\n", "\n"), searchResult.LyricText.Replace("\r\n", "\n"));
        }

        // Synthetic pages with only the markup the provider depends on (#69).
        private const string ArtistPage = """
            <html><body>
            <a href="/t/test+artist/" title="Test Artist Lyrics">Test Artist</a>
            <a class="lf-link" href="/t/test+artist/test+song+live_1.html" title="Test Song (Live) Lyrics">Test Song (Live)</a>
            <a class="lf-link" href="/t/test+artist/dont+stop_2.html" title="Don&#039;t Stop Lyrics">Don't Stop</a>
            <a class="lf-link" href="/t/test+artist/avohai_3.html" title="Av&ocirc;hai Lyrics">Avôhai</a>
            <a class="lf-link" href="/t/test+artist/harder+better_4.html" title="Harder, Better Lyrics">Harder, Better</a>
            <a href="/t/test+artist/test+song+video_6.html" title="Test Song (Video)">not a lyrics link</a>
            <a class="lf-link" href="/t/test+artist/test+song_5.html" title="Test Song Lyrics">Test Song</a>
            <a href="/t/test+artist/test+song_5.html" title="Test Song">no Lyrics suffix</a>
            </body></html>
            """;
        private const string LyricPage = "<html><body><div data-container-id='lyrics'>Test line one\nTest line two</div></body></html>";
        private const string NotFoundPage = "<html><body><h1>#404 - Page Not Found</h1></body></html>";
        private static readonly Uri ArtistUri = new("https://www.lyricsfreak.com/t/test+artist");
        private static readonly Uri BaseLetterArtistUri = new("https://www.lyricsfreak.com/t/teste+artist");
        private static readonly Uri TestSongUri = new("https://www.lyricsfreak.com/t/test+artist/test+song_5.html");

        [Theory]
        [InlineData("Test Song", "/t/test+artist/test+song_5.html")]
        [InlineData("TEST SONG", "/t/test+artist/test+song_5.html")]
        [InlineData("Don't Stop", "/t/test+artist/dont+stop_2.html")]
        [InlineData("Dont Stop", "/t/test+artist/dont+stop_2.html")]
        [InlineData("Avohai", "/t/test+artist/avohai_3.html")]
        [InlineData("Avôhai", "/t/test+artist/avohai_3.html")]
        [InlineData("Harder Better", "/t/test+artist/harder+better_4.html")]
        [InlineData("Test Song (Live)", "/t/test+artist/test+song+live_1.html")]
        public async Task SearchLyricAsync_SongWrittenDifferently_LoadsItsPage(string song, string expectedHref)
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var songUri = new Uri(LyricsFreakUriConverter.BaseUrl + expectedHref);
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(ArtistUri, cancellationToken)).Returns(ArtistPage);
            A.CallTo(() => webClient.LoadAsync(songUri, cancellationToken)).Returns(LyricPage);

            var lyricsClient = new LyricsFreakProvider();
            lyricsClient.WithWebClient(webClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest("Test Artist", song), cancellationToken);

            // Assert
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.Equal(ExternalProviderType.LyricsFreak, searchResult.ExternalProviderType);
            Assert.Equal("Test line one\nTest line two", searchResult.LyricText);
            A.CallTo(() => webClient.LoadAsync(ArtistUri, cancellationToken)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => webClient.LoadAsync(songUri, cancellationToken)).MustHaveHappenedOnceExactly());
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustHaveHappenedTwiceExactly();
        }

        [Theory]
        // A part of the title used to be enough: "Song" found "Test Song".
        [InlineData("Song")]
        [InlineData("Another Song")]
        [InlineData("Test Song (Remix)")]
        public async Task SearchLyricAsync_NoSongWithSameTitle_ReturnsNoDataFoundWithoutLoadingSongPage(string song)
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(ArtistUri, cancellationToken)).Returns(ArtistPage);

            var lyricsClient = new LyricsFreakProvider();
            lyricsClient.WithWebClient(webClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest("Test Artist", song), cancellationToken);

            // Assert
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            A.CallTo(() => webClient.LoadAsync(ArtistUri, cancellationToken)).MustHaveHappenedOnceExactly();
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Theory]
        // 404 is an empty body (NetHttpClient) or a page with the 404 text.
        [InlineData("")]
        [InlineData(NotFoundPage)]
        public async Task SearchLyricAsync_BaseLetterArtistPageNotFound_TriesSlugWithDroppedLetters(string notFoundResponse)
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(BaseLetterArtistUri, cancellationToken)).Returns(notFoundResponse);
            A.CallTo(() => webClient.LoadAsync(ArtistUri, cancellationToken)).Returns(ArtistPage);
            A.CallTo(() => webClient.LoadAsync(TestSongUri, cancellationToken)).Returns(LyricPage);

            var lyricsClient = new LyricsFreakProvider();
            lyricsClient.WithWebClient(webClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest("Testé Artist", "Test Song"), cancellationToken);

            // Assert
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.Equal("Test line one\nTest line two", searchResult.LyricText);
            A.CallTo(() => webClient.LoadAsync(BaseLetterArtistUri, cancellationToken)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => webClient.LoadAsync(ArtistUri, cancellationToken)).MustHaveHappenedOnceExactly())
                .Then(A.CallTo(() => webClient.LoadAsync(TestSongUri, cancellationToken)).MustHaveHappenedOnceExactly());
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustHaveHappened(3, Times.Exactly);
        }

        [Fact]
        public async Task SearchLyricAsync_BaseLetterArtistPageFound_DoesNotTryOtherSlug()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(BaseLetterArtistUri, cancellationToken)).Returns(ArtistPage);
            A.CallTo(() => webClient.LoadAsync(TestSongUri, cancellationToken)).Returns(LyricPage);

            var lyricsClient = new LyricsFreakProvider();
            lyricsClient.WithWebClient(webClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest("Testé Artist", "Test Song"), cancellationToken);

            // Assert
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            A.CallTo(() => webClient.LoadAsync(BaseLetterArtistUri, cancellationToken)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => webClient.LoadAsync(TestSongUri, cancellationToken)).MustHaveHappenedOnceExactly());
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task SearchLyricAsync_AllArtistPagesNotFound_ReturnsNoDataFound()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).Returns(NotFoundPage);

            var lyricsClient = new LyricsFreakProvider();
            lyricsClient.WithWebClient(webClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest("Testé Artist", "Test Song"), cancellationToken);

            // Assert
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            A.CallTo(() => webClient.LoadAsync(BaseLetterArtistUri, cancellationToken)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => webClient.LoadAsync(ArtistUri, cancellationToken)).MustHaveHappenedOnceExactly());
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustHaveHappenedTwiceExactly();
        }

        [Theory]
        [InlineData("Кино")]
        [InlineData("방탄소년단")]
        public async Task SearchLyricAsync_ArtistWithoutLatinLetters_ReturnsNoDataFoundWithoutRequests(string artist)
        {
            // Arrange
            var webClient = A.Fake<IWebClient>();
            var lyricsClient = new LyricsFreakProvider();
            lyricsClient.WithWebClient(webClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest(artist, "Song"), TestContext.Current.CancellationToken);

            // Assert
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.Equal(ExternalProviderType.LyricsFreak, searchResult.ExternalProviderType);
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustNotHaveHappened();
        }
    }
}
