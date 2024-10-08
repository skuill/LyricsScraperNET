﻿using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.LyricFind;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.TestShared.Providers;
using LyricsScraperNET.UnitTest.TestModel;
using Xunit;

namespace LyricsScraperNET.IntegrationTest.Providers.LyricFind
{
    public class LyricFindProviderTest : ProviderTestBase
    {
        [Theory]
        [MemberData(nameof(GetTestData), parameters: "Providers\\LyricFind\\lyric_test_data.json")]
        public void SearchLyric_IntegrationDynamicData_Success(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new LyricFindProvider();
            SearchRequest searchRequest = CreateSearchRequest(testData);

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.False(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.LyricFind, searchResult.ExternalProviderType);
            Assert.Equal(testData.LyricResultData.Replace("\r\n", "\n"), searchResult.LyricText.Replace("\r\n", "\n"));
            Assert.False(searchResult.Instrumental);
        }

        [Theory]
        [MemberData(nameof(GetTestData), parameters: "Providers\\LyricFind\\instrumental_test_data.json")]
        public void SearchLyric_IntegrationDynamicData_Instrumental(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new LyricFindProvider();
            SearchRequest searchRequest = CreateSearchRequest(testData);

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.LyricFind, searchResult.ExternalProviderType);
            Assert.True(searchResult.Instrumental);
        }

        [Theory]
        [InlineData("asdfasdfasdfasdf", "asdfasdfasdfasdf")]
        public void SearchLyric_NotExistsLyrics_ShouldReturnNoDataFoundStatus(string artist, string song)
        {
            // Arrange
            var lyricsClient = new LyricFindProvider();
            var searchRequest = new ArtistAndSongSearchRequest(artist, song);

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.Equal(ExternalProviderType.LyricFind, searchResult.ExternalProviderType);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.False(searchResult.Instrumental);
        }
    }
}
