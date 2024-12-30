using System.Threading;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.KPopLyrics;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.TestShared.Extensions;
using LyricsScraperNET.TestShared.Providers;
using LyricsScraperNET.TestShared.TestModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LyricsScraperNET.UnitTest.Providers.KPopLyrics
{
    public class KPopLyricsProviderTest : ProviderTestBase
    {
        [Theory]
        [MemberData(nameof(GetTestData), parameters: "Providers\\KPopLyrics\\lyric_test_data.json")]
        public void SearchLyric_UnitDynamicData_Success(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new KPopLyricsProvider();
            lyricsClient.ConfigureExternalProvider(testData);

            var searchRequest = CreateSearchRequest(testData);
            var cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.False(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.KPopLyrics, searchResult.ExternalProviderType);
            Assert.Equal(testData.LyricResultData.Replace("\r", string.Empty), searchResult.LyricText);
        }
    }
}