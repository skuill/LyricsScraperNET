using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.Providers.SongLyrics;
using LyricsScraperNET.TestShared.Extensions;
using LyricsScraperNET.TestShared.Providers;
using LyricsScraperNET.TestShared.TestModel;
using System.Threading;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.SongLyrics
{
    public class SongLyricsProviderTest : ProviderTestBase
    {
        [Theory]
        [MemberData(nameof(GetTestData), parameters: "Providers\\SongLyrics\\lyric_test_data.json")]
        public void SearchLyric_UnitDynamicData_Success(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new SongLyricsProvider();
            lyricsClient.ConfigureExternalProvider(testData);

            SearchRequest searchRequest = CreateSearchRequest(testData);
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.SongLyrics, searchResult.ExternalProviderType);
            Assert.Equal(testData.LyricResultData.Replace("\r\n", "\n"), searchResult.LyricText.Replace("\r\n", "\n"));
            Assert.False(searchResult.Instrumental);
        }

        [Theory]
        [MemberData(nameof(GetTestData), parameters: "Providers\\SongLyrics\\instrumental_test_data.json")]
        public void SearchLyric_UnitDynamicData_Instrumental(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new SongLyricsProvider();
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
            Assert.Equal(ExternalProviderType.SongLyrics, searchResult.ExternalProviderType);
            Assert.True(searchResult.Instrumental);
        }
    }
}
