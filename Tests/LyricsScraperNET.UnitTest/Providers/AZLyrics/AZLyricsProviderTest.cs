using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.UnitTest.TestModel;
using LyricsScraperNET.UnitTest.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace LyricsScraperNET.UnitTest.Providers.AZLyrics
{
    [TestClass]
    public class AZLyricsProviderTest
    {
        private static readonly string[] TEST_DATA_PATH = { "Providers", "AZLyrics", "test_data.json" };

        [TestMethod]
        [DynamicData(nameof(GetTestData), DynamicDataSourceType.Method)]
        public void SearchLyric_UnitDynamicData_AreEqual(LyricsTestData testData)
        {
            // Arrange
            var mockWebClient = new Mock<IWebClient>();
            mockWebClient.Setup(x => x.Load(It.IsAny<Uri>())).Returns(testData.LyricPageData);

            var lyricsClient = new AZLyricsProvider();
            lyricsClient.WithWebClient(mockWebClient.Object);

            SearchRequest searchRequest = !string.IsNullOrEmpty(testData.SongUri)
                ? new UriSearchRequest(testData.SongUri)
                : new ArtistAndSongSearchRequest(testData.ArtistName, testData.SongName);

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest);

            // Assert
            Assert.IsNotNull(searchResult);
            Assert.AreEqual(ExternalProviderType.AZLyrics, searchResult.ExternalProviderType);
            Assert.AreEqual(testData.LyricResultData.Replace("\r\n", "\n"), searchResult.LyricText.Replace("\r\n", "\n"));
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
