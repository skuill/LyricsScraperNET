using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.LyricFind;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.TestShared.Providers;
using LyricsScraperNET.UnitTest.TestModel;
using System.Threading.Tasks;
using System.Threading;
using Xunit;

namespace LyricsScraperNET.IntegrationTest.Providers.LyricFind
{
    public class LyricFindProviderTest : ProviderTestBase
    {
        #region sync

        [Theory]
        [MemberData(nameof(GetTestData), parameters: "Providers\\LyricFind\\lyric_test_data.json")]
        public void SearchLyric_IntegrationDynamicData_Success(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new LyricFindProvider();
            SearchRequest searchRequest = CreateSearchRequest(testData);
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.False(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.LyricFind, searchResult.ExternalProviderType);
            Assert.Equal(testData.LyricResultData.Replace("\r\n", "\n"), searchResult.LyricText.Replace("\r\n", "\n"));
            Assert.False(searchResult.Instrumental);
        }

        [RegionalTestTheory(excludeRegions: new[] { "AM", "RU" })]
        [InlineData("rush", "yyz")]
        public void SearchLyric_Instrumental_ShouldReturnSuccess(string artist, string song)
        {
            // Arrange
            var lyricsClient = new LyricFindProvider();
            var searchRequest = new ArtistAndSongSearchRequest(artist, song);
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.LyricFind, searchResult.ExternalProviderType);
            Assert.True(searchResult.Instrumental);
        }

        [RegionalTestTheory(includeRegions: new[] { "AM", "RU" })]
        [InlineData("rush", "Tom Sawyer")]
        public void SearchLyric_LyricAreNotAvailableInRegion_ShouldRegionRestrictedStatus(string artist, string song)
        {
            // Arrange
            var lyricsClient = new LyricFindProvider();
            var searchRequest = new ArtistAndSongSearchRequest(artist, song);

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.RegionRestricted, searchResult.ResponseStatusCode);
            Assert.Equal(ExternalProviderType.LyricFind, searchResult.ExternalProviderType);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.False(searchResult.Instrumental);
        }

        [Theory]
        [InlineData("asdfasdfasdfasdf", "asdfasdfasdfasdf")]
        public void SearchLyric_NotExistsLyrics_ShouldReturnNoDataFoundStatus(string artist, string song)
        {
            // Arrange
            var lyricsClient = new LyricFindProvider();
            var searchRequest = new ArtistAndSongSearchRequest(artist, song);
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.Equal(ExternalProviderType.LyricFind, searchResult.ExternalProviderType);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.False(searchResult.Instrumental);
        }

        #endregion

        #region async

        [Theory]
        [MemberData(nameof(GetTestData), parameters: "Providers\\LyricFind\\lyric_test_data.json")]
        public async Task SearchLyricAsync_IntegrationDynamicData_Success(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new LyricFindProvider();
            SearchRequest searchRequest = CreateSearchRequest(testData);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.False(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.LyricFind, searchResult.ExternalProviderType);
            Assert.Equal(testData.LyricResultData.Replace("\r\n", "\n"), searchResult.LyricText.Replace("\r\n", "\n"));
            Assert.False(searchResult.Instrumental);
        }

        [RegionalTestTheory(excludeRegions: new[] { "AM", "RU" })]
        [InlineData("rush", "yyz")]
        public async Task SearchLyricAsync_Instrumental_ShouldReturnSuccess(string artist, string song)
        {
            // Arrange
            var lyricsClient = new LyricFindProvider();
            var searchRequest = new ArtistAndSongSearchRequest(artist, song);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.LyricFind, searchResult.ExternalProviderType);
            Assert.True(searchResult.Instrumental);
        }

        [RegionalTestTheory(includeRegions: new[] { "AM", "RU" })]
        [InlineData("rush", "Tom Sawyer")]
        public async Task SearchLyricAsync_LyricAreNotAvailableInRegion_ShouldRegionRestrictedStatus(string artist, string song)
        {
            // Arrange
            var lyricsClient = new LyricFindProvider();
            var searchRequest = new ArtistAndSongSearchRequest(artist, song);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.RegionRestricted, searchResult.ResponseStatusCode);
            Assert.Equal(ExternalProviderType.LyricFind, searchResult.ExternalProviderType);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.False(searchResult.Instrumental);
        }

        [Theory]
        [InlineData("asdfasdfasdfasdf", "asdfasdfasdfasdf")]
        public async Task SearchLyricAsync_NotExistsLyrics_ShouldReturnNoDataFoundStatus(string artist, string song)
        {
            // Arrange
            var lyricsClient = new LyricFindProvider();
            var searchRequest = new ArtistAndSongSearchRequest(artist, song);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.Equal(ExternalProviderType.LyricFind, searchResult.ExternalProviderType);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.False(searchResult.Instrumental);
        }

        #endregion
    }
}
