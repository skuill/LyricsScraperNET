﻿using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.TestShared.Providers;
using LyricsScraperNET.UnitTest.TestModel;
using Moq;
using System;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.AZLyrics
{
    public class AZLyricsProviderTest : ProviderTestBase
    {
        [Theory]
        [MemberData(nameof(GetTestData), parameters: "Providers\\AZLyrics\\test_data.json")]
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
            Assert.NotNull(searchResult);
            Assert.Equal(ExternalProviderType.AZLyrics, searchResult.ExternalProviderType);
            Assert.Equal(testData.LyricResultData.Replace("\r\n", "\n"), searchResult.LyricText.Replace("\r\n", "\n"));
        }
    }
}
