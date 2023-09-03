using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.Providers.SongLyrics;
using LyricsScraperNET.TestShared.Providers;
using LyricsScraperNET.UnitTest.TestModel;
using Xunit;

namespace LyricsScraperNET.IntegrationTest.Providers.SongLyrics
{
    public class SongLyricsProviderTest : ProviderTestBase
    {
        [Theory]
        [MemberData(nameof(GetTestData), parameters: "Providers\\SongLyrics\\test_data.json")]
        public void SearchLyric_IntegrationDynamicData_AreEqual(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new SongLyricsProvider();
            SearchRequest searchRequest = CreateSearchRequest(testData);

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.False(searchResult.IsEmpty());
            Assert.Equal(ExternalProviderType.SongLyrics, searchResult.ExternalProviderType);
            Assert.Equal(testData.LyricResultData.Replace("\r\n", "\n"), searchResult.LyricText.Replace("\r\n", "\n"));
        }
    }
}
