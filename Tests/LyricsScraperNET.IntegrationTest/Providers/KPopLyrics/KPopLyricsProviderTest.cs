using System;
using System.Threading;
using System.Threading.Tasks;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.KPopLyrics;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.TestShared.Providers;
using LyricsScraperNET.TestShared.TestModel;
using Xunit;

namespace LyricsScraperNET.IntegrationTest.Providers.KPopLyrics
{
    public class KPopLyricsProviderTest(ITestOutputHelper testOutputHelper) : ProviderTestBase
    {
        #region sync

        [Theory]
        [MemberData(nameof(GetTestData), arguments: "Providers\\KPopLyrics\\lyric_test_data.json")]
        public void SearchLyric_IntegrationDynamicData_Success(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new KPopLyricsProvider();
            SearchRequest searchRequest = CreateSearchRequest(testData);
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.False(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.KPopLyrics, searchResult.ExternalProviderType);
            Assert.Equal(testData.LyricResultData.Replace("\r\n", "\n"), searchResult.LyricText.Replace("\r\n", "\n"));
            testOutputHelper.WriteLine(testData.LyricResultData.Length.ToString());
            testOutputHelper.WriteLine(searchResult.LyricText.Length.ToString());
        }

        [Theory]
        [InlineData("asdfasdfasdfasdf", "asdfasdfasdfasdf")]
        public void SearchLyric_NotExistsLyrics_ShouldReturnNoDataFoundStatus(string artist, string song)
        {
            // Arrange
            var lyricsClient = new KPopLyricsProvider();
            var searchRequest = new ArtistAndSongSearchRequest(artist, song);
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.Equal(ExternalProviderType.KPopLyrics, searchResult.ExternalProviderType);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.False(searchResult.Instrumental);
        }

        #endregion

        #region async

        [Theory]
        [MemberData(nameof(GetTestData), arguments: "Providers\\KPopLyrics\\lyric_test_data.json")]
        public async Task SearchLyricAsync_IntegrationDynamicData_Success(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new KPopLyricsProvider();
            SearchRequest searchRequest = CreateSearchRequest(testData);
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.False(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.KPopLyrics, searchResult.ExternalProviderType);
            Assert.Equal(testData.LyricResultData.Replace("\r\n", "\n"), searchResult.LyricText.Replace("\r\n", "\n"));
        }

        [Theory]
        [InlineData("asdfasdfasdfasdf", "asdfasdfasdfasdf")]
        public async Task SearchLyricAsync_NotExistsLyrics_ShouldReturnNoDataFoundStatus(string artist, string song)
        {
            // Arrange
            var lyricsClient = new KPopLyricsProvider();
            var searchRequest = new ArtistAndSongSearchRequest(artist, song);
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.Equal(ExternalProviderType.KPopLyrics, searchResult.ExternalProviderType);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.False(searchResult.Instrumental);
        }

        [Fact]
        public async Task SearchLyricAsync_SongWithRemixPost_ReturnsTheSongNotTheRemix()
        {
            // Arrange: a guessed URL used to be redirected to "BTS – Dynamite (Holiday Remix)" (#56).
            var lyricsClient = new KPopLyricsProvider();
            var songUri = new Uri("https://www.kpoplyrics.net/bts-dynamite-lyrics-english-romanized.html");
            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest("BTS", "Dynamite"), cancellationToken);
            var songPageResult = await lyricsClient.SearchLyricAsync(new UriSearchRequest(songUri), cancellationToken);

            // Assert
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.Equal(ResponseStatusCode.Success, songPageResult.ResponseStatusCode);
            Assert.Equal(songPageResult.LyricText, searchResult.LyricText);
        }

        #endregion
    }
}