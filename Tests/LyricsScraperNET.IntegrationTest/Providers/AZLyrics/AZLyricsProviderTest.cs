using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.TestShared.Providers;
using LyricsScraperNET.UnitTest.TestModel;
using Xunit;

namespace LyricsScraperNET.IntegrationTest.Providers.AZLyrics
{
    public class AZLyricsProviderTest : ProviderTestBase
    {
        [Theory]
        [MemberData(nameof(GetTestData), parameters: "Providers\\AZLyrics\\test_data.json")]
        public void SearchLyric_IntegrationDynamicData_AreEqual(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new AZLyricsProvider();
            SearchRequest searchRequest = CreateSearchRequest(testData);

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.False(searchResult.IsEmpty());
            Assert.Equal(ExternalProviderType.AZLyrics, searchResult.ExternalProviderType);
            Assert.Equal(testData.LyricResultData.Replace("\r\n", "\n"), searchResult.LyricText.Replace("\r\n", "\n"));
        }
    }
}
