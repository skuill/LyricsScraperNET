using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Providers.Genius;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.UnitTest.TestModel;
using LyricsScraperNET.UnitTest.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace LyricsScraperNET.IntegrationTest.Providers.Genius
{
    [TestClass]
    public class GeniusProviderTest
    {
        private static readonly string[] TEST_DATA_PATH = { "Providers", "Genius", "test_data.json" };

        [TestMethod]
        [DynamicData(nameof(GetTestData), DynamicDataSourceType.Method)]
        public void SearchLyric_IntegrationDynamicData_AreEqual(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new GeniusProvider();

            SearchRequest searchRequest = !string.IsNullOrEmpty(testData.SongUri)
                ? new UriSearchRequest(testData.SongUri)
                : new ArtistAndSongSearchRequest(testData.ArtistName, testData.SongName);

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest);

            // Assert
            Assert.IsNotNull(searchResult);
            Assert.IsFalse(searchResult.IsEmpty());
            Assert.AreEqual(ExternalProviderType.Genius, searchResult.ExternalProviderType);
            Assert.AreEqual(testData.LyricResultData, searchResult.LyricText);
        }

        public static IEnumerable<object[]> GetTestData()
        {
            foreach (var testData in Serializer.Deseialize<List<LyricsTestData>>(TEST_DATA_PATH))
            {
                yield return new object[] { testData };
            }
        }
    }
}
