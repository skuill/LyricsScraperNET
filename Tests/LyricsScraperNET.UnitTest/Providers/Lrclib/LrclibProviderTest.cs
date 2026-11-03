using FakeItEasy;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Providers.Lrclib;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.TestShared.Extensions;
using LyricsScraperNET.TestShared.Providers;
using LyricsScraperNET.TestShared.TestModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.Lrclib
{
    public class LrclibProviderTest : ProviderTestBase
    {
        [Theory]
        [MemberData(nameof(GetTestData), arguments: "Providers\\Lrclib\\lyric_test_data.json")]
        public void SearchLyric_UnitDynamicData_Success(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new LrclibProvider();
            lyricsClient.ConfigureExternalProvider(testData);

            SearchRequest searchRequest = CreateSearchRequest(testData);
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.Lrclib, searchResult.ExternalProviderType);
            Assert.Equal(testData.LyricResultData.Replace("\r\n", "\n"), searchResult.LyricText.Replace("\r\n", "\n"));
            Assert.False(searchResult.Instrumental);
        }

        [Theory]
        [MemberData(nameof(GetTestData), arguments: "Providers\\Lrclib\\instrumental_test_data.json")]
        public void SearchLyric_UnitDynamicData_Instrumental(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new LrclibProvider();
            lyricsClient.ConfigureExternalProvider(testData);

            SearchRequest searchRequest = CreateSearchRequest(testData);
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.Lrclib, searchResult.ExternalProviderType);
            Assert.True(searchResult.Instrumental);
        }

        [Theory]
        [MemberData(nameof(GetTestData), arguments: "Providers\\Lrclib\\not_found_test_data.json")]
        public void SearchLyric_UnitDynamicData_NotFound(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new LrclibProvider();
            lyricsClient.ConfigureExternalProvider(testData);

            SearchRequest searchRequest = CreateSearchRequest(testData);

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.Equal(ExternalProviderType.Lrclib, searchResult.ExternalProviderType);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.False(searchResult.Instrumental);
        }

        private const string TrackNotFoundJson = "{\"code\":404,\"name\":\"TrackNotFound\",\"message\":\"Failed to find specified track\"}";
        private const string TrackJson = "{\"id\":1,\"instrumental\":false,\"plainLyrics\":\"Test line one\\nTest line two\"}";

        [Theory]
        [InlineData("Test Artist", "Rock & Roll", "Test Artist", "Rock and Roll")]
        [InlineData("Test & Artist", "Rock and Roll", "Test and Artist", "Rock and Roll")]
        [InlineData("Test and Artist", "Song", "Test & Artist", "Song")]
        [InlineData("Test Artist", "Rock AND Roll", "Test Artist", "Rock & Roll")]
        public async Task SearchLyricAsync_TrackNotFoundWithAmpersandOrAnd_RetriesOnceWithOtherSpelling(string artist, string song, string otherArtist, string otherSong)
        {
            // Arrange
            var uriConverter = new LrclibUriConverter();
            var firstUri = uriConverter.GetLyricUri(artist, song);
            var otherUri = uriConverter.GetLyricUri(otherArtist, otherSong);
            var cancellationToken = TestContext.Current.CancellationToken;

            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(firstUri, cancellationToken)).Returns(TrackNotFoundJson);
            A.CallTo(() => webClient.LoadAsync(otherUri, cancellationToken)).Returns(TrackJson);

            var lyricsClient = new LrclibProvider();
            lyricsClient.WithWebClient(webClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest(artist, song), cancellationToken);

            // Assert
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.Equal(ExternalProviderType.Lrclib, searchResult.ExternalProviderType);
            Assert.Equal("Test line one\nTest line two", searchResult.LyricText);
            A.CallTo(() => webClient.LoadAsync(firstUri, cancellationToken)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => webClient.LoadAsync(otherUri, cancellationToken)).MustHaveHappenedOnceExactly());
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustHaveHappenedTwiceExactly();
        }

        [Theory]
        // Neither "&" nor the word "and": nothing to swap ("Andy", "Band" are not the word).
        [InlineData(TrackNotFoundJson, "Andy Band", "Song")]
        // Not TrackNotFound: an empty body is a failed load, the other spelling would not help.
        [InlineData("", "Test & Artist", "Song")]
        [InlineData(TrackJson, "Test & Artist", "Song")]
        public async Task SearchLyricAsync_NotTrackNotFoundOrNothingToSwap_LoadsOnce(string response, string artist, string song)
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).Returns(response);

            var lyricsClient = new LrclibProvider();
            lyricsClient.WithWebClient(webClient);

            // Act
            await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest(artist, song), cancellationToken);

            // Assert
            A.CallTo(() => webClient.LoadAsync(new LrclibUriConverter().GetLyricUri(artist, song), cancellationToken)).MustHaveHappenedOnceExactly();
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SearchLyricAsync_TrackNotFoundForBothSpellings_ReturnsNoDataFound()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).Returns(TrackNotFoundJson);

            var lyricsClient = new LrclibProvider();
            lyricsClient.WithWebClient(webClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest("Test & Artist", "Song"), cancellationToken);

            // Assert
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.Equal(ExternalProviderType.Lrclib, searchResult.ExternalProviderType);
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task SearchLyricAsync_ByUriTrackNotFound_DoesNotRetry()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var uri = new LrclibUriConverter().GetLyricUri("Test & Artist", "Song");
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).Returns(TrackNotFoundJson);

            var lyricsClient = new LrclibProvider();
            lyricsClient.WithWebClient(webClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new UriSearchRequest(uri), cancellationToken);

            // Assert
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            A.CallTo(() => webClient.LoadAsync(uri, cancellationToken)).MustHaveHappenedOnceExactly();
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }
    }
}
