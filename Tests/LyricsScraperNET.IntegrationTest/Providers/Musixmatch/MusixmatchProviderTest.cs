using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.Providers.Musixmatch;
using LyricsScraperNET.TestShared.Providers;
using LyricsScraperNET.TestShared.TestModel;
using System.Threading.Tasks;
using System.Threading;
using Xunit;

namespace LyricsScraperNET.IntegrationTest.Providers.Musixmatch
{
    public class MusixmatchProviderTest : ProviderTestBase
    {
        #region sync

        [Theory]
        [MemberData(nameof(GetTestData), parameters: "Providers\\Musixmatch\\lyric_test_data.json")]
        public void SearchLyric_IntegrationDynamicData_Success(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new MusixmatchProvider();
            SearchRequest searchRequest = CreateSearchRequest(testData);
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.False(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.Musixmatch, searchResult.ExternalProviderType);
            Assert.Equal(testData.LyricResultData.Replace("\r\n", "\n"), searchResult.LyricText);
            Assert.False(searchResult.Instrumental);
        }

        [Theory]
        [MemberData(nameof(GetTestData), parameters: "Providers\\Musixmatch\\instrumental_test_data.json")]
        public void SearchLyric_IntegrationDynamicData_Instrumental(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new MusixmatchProvider();
            SearchRequest searchRequest = CreateSearchRequest(testData);
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.Musixmatch, searchResult.ExternalProviderType);
            Assert.True(searchResult.Instrumental);
        }

        [Theory]
        [InlineData("asdfasdfasdfasdf", "asdfasdfasdfasdf")]
        public void SearchLyric_NotExistsLyrics_ShouldReturnNoDataFoundStatus(string artist, string song)
        {
            // Arrange
            var lyricsClient = new MusixmatchProvider();
            var searchRequest = new ArtistAndSongSearchRequest(artist, song);
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.Equal(ExternalProviderType.Musixmatch, searchResult.ExternalProviderType);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.False(searchResult.Instrumental);
        }

        #endregion

        #region async

        [Theory]
        [MemberData(nameof(GetTestData), parameters: "Providers\\Musixmatch\\lyric_test_data.json")]
        public async Task SearchLyricAsync_IntegrationDynamicData_Success(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new MusixmatchProvider();
            SearchRequest searchRequest = CreateSearchRequest(testData);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.False(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.Musixmatch, searchResult.ExternalProviderType);
            Assert.Equal(testData.LyricResultData.Replace("\r\n", "\n"), searchResult.LyricText);
            Assert.False(searchResult.Instrumental);
        }

        [Theory]
        [MemberData(nameof(GetTestData), parameters: "Providers\\Musixmatch\\instrumental_test_data.json")]
        public async Task SearchLyricAsync_IntegrationDynamicData_Instrumental(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new MusixmatchProvider();
            SearchRequest searchRequest = CreateSearchRequest(testData);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.Musixmatch, searchResult.ExternalProviderType);
            Assert.True(searchResult.Instrumental);
        }

        [Theory]
        [InlineData("asdfasdfasdfasdf", "asdfasdfasdfasdf")]
        public async Task SearchLyricAsync_NotExistsLyrics_ShouldReturnNoDataFoundStatus(string artist, string song)
        {
            // Arrange
            var lyricsClient = new MusixmatchProvider();
            var searchRequest = new ArtistAndSongSearchRequest(artist, song);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.Equal(ExternalProviderType.Musixmatch, searchResult.ExternalProviderType);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.False(searchResult.Instrumental);
        }

        #endregion
    }
}