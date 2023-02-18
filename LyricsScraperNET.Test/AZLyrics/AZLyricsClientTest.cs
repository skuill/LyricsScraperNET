using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.Test.TestModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace LyricsScraperNET.Test.AZLyrics
{
    [TestClass]
    public class AZLyricsClientTest
    {
        private readonly string[] TEST_DATA_PATH = { "AZLyrics", "test_data.json" };
        private List<LyricsTestData> _testDataCollection;

        [TestInitialize]
        public void TestInitialize()
        {
            _testDataCollection = Serializer.Deseialize<List<LyricsTestData>>(TEST_DATA_PATH);
        }

        [TestMethod]
        public void SearchLyric_MockWebClient_AreEqual()
        {

            foreach (var testData in _testDataCollection)
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
                Assert.AreEqual(testData.LyricResultData, searchResult.LyricText);
            }
        }
    }
}
