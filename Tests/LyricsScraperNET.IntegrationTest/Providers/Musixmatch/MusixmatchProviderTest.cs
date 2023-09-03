using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.Providers.Musixmatch;
using LyricsScraperNET.TestShared.Providers;
using LyricsScraperNET.UnitTest.TestModel;
using Xunit;

namespace LyricsScraperNET.IntegrationTest.Providers.Musixmatch
{
    public class MusixmatchProviderTest : ProviderTestBase
    {
        [Theory]
        [MemberData(nameof(GetTestData), parameters: "Providers\\Musixmatch\\test_data.json")]
        public void SearchLyric_IntegrationDynamicData_AreEqual(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new MusixmatchProvider();
            SearchRequest searchRequest = CreateSearchRequest(testData);

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.False(searchResult.IsEmpty());
            Assert.Equal(ExternalProviderType.Musixmatch, searchResult.ExternalProviderType);
            Assert.Equal(testData.LyricResultData.Replace("\r\n", "\n"), searchResult.LyricText);
        }
    }
}
