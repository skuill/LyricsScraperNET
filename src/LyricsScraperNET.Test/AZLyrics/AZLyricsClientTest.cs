using LyricsScraperNET.External.AZLyrics;
using LyricsScraperNET.Models;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Test.TestModel;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace LyricsScraperNET.Test.AZLyrics
{
    [TestClass]
    public class AZLyricsClientTest
    {
        private const string TEST_DATA_PATH = "AZLyrics\\test_data.json";
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

                var options = Options.Create(new AZLyricsOptions() { Enabled = true });

                var lyricsClient = new AZLyricsClient(null, options);
                lyricsClient.WithWebClient(mockWebClient.Object);

                SearchRequest searchRequest = !string.IsNullOrEmpty(testData.SongUri)
                    ? new UriSearchRequest(testData.SongUri)
                    : new ArtistAndSongSearchRequest(testData.ArtistName, testData.SongName);

                // Act
                var lyric = lyricsClient.SearchLyric(searchRequest);

                // Assert
                Assert.AreEqual(testData.LyricResultData, lyric);
            }
        }
    }
}
