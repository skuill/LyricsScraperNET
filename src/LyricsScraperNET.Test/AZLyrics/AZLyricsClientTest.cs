using LyricsScraper.AZLyrics;
using LyricsScraper.Network.Abstract;
using LyricsScraper.Network.Html;
using LyricsScraper.Test.TestModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace LyricsScraper.Test.AZLyrics
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
                var mockWebClient = new Mock<ILyricWebClient>();
                mockWebClient.Setup(x => x.Load(It.IsAny<Uri>())).Returns(testData.LyricPageData);

                var lyricsClient = new AZLyricsClient(null, new AZLyricsParser(), new HtmlAgilityWebClient());
                lyricsClient.WithWebClient(mockWebClient.Object);

                // Act
                var lyric = !string.IsNullOrEmpty(testData.SongUri) 
                    ? lyricsClient.SearchLyric(new Uri(testData.SongUri)) 
                    : lyricsClient.SearchLyric(testData.ArtistName, testData.SongName);

                // Assert
                Assert.AreEqual(testData.LyricResultData, lyric);
            }
        }
    }
}
