using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.LyricFind;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.TestShared.Extensions;
using LyricsScraperNET.TestShared.Providers;
using LyricsScraperNET.UnitTest.TestModel;
using System.Threading;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.LyricFind
{
    public class LyricFindProviderTest : ProviderTestBase
    {
        [Theory]
        [MemberData(nameof(GetTestData), parameters: "Providers\\LyricFind\\lyric_test_data.json")]
        public void SearchLyric_UnitDynamicData_Success(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new LyricFindProvider();
            lyricsClient.ConfigureExternalProvider(testData);

            SearchRequest searchRequest = CreateSearchRequest(testData);
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.LyricFind, searchResult.ExternalProviderType);
            Assert.Equal(testData.LyricResultData.Replace("\r\n", "\n"), searchResult.LyricText.Replace("\r\n", "\n"));
            Assert.False(searchResult.Instrumental);
        }

        [Theory]
        [MemberData(nameof(GetTestData), parameters: "Providers\\LyricFind\\instrumental_test_data.json")]
        public void SearchLyric_UnitDynamicData_Instrumental(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new LyricFindProvider();
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
            Assert.Equal(ExternalProviderType.LyricFind, searchResult.ExternalProviderType);
            Assert.True(searchResult.Instrumental);
        }

        [Theory]
        [MemberData(nameof(GetTestData), parameters: "Providers\\LyricFind\\region_restricted_test_data.json")]
        public void SearchLyric_UnitDynamicData_RegionRestricted(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new LyricFindProvider();
            lyricsClient.ConfigureExternalProvider(testData);

            SearchRequest searchRequest = CreateSearchRequest(testData);

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
        [MemberData(nameof(GetTestData), parameters: "Providers\\LyricFind\\not_found_test_data.json")]
        public void SearchLyric_UnitDynamicData_NotFound(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new LyricFindProvider();
            lyricsClient.ConfigureExternalProvider(testData);

            SearchRequest searchRequest = CreateSearchRequest(testData);

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.Equal(ExternalProviderType.LyricFind, searchResult.ExternalProviderType);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.False(searchResult.Instrumental);
        }
    }
}
