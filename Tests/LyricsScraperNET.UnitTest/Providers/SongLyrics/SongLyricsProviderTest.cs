using FakeItEasy;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.Providers.SongLyrics;
using LyricsScraperNET.TestShared.Extensions;
using LyricsScraperNET.TestShared.Providers;
using LyricsScraperNET.TestShared.TestModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.SongLyrics
{
    public class SongLyricsProviderTest : ProviderTestBase
    {
        [Fact]
        public void Constructor_Default_UsesNetHttpClient()
        {
            // Act
            var lyricsClient = new SongLyricsProvider();

            // Assert
            Assert.IsType<NetHttpClient>(lyricsClient.WebClient);
        }

        [Theory]
        [MemberData(nameof(GetTestData), arguments: "Providers\\SongLyrics\\lyric_test_data.json")]
        public async Task SearchLyricAsync_UnitDynamicData_LoadsLyricUriAndReturnsSuccess(LyricsTestData testData)
        {
            // Arrange
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).Returns(testData.LyricPageData);

            var lyricsClient = new SongLyricsProvider();
            lyricsClient.WithWebClient(webClient);

            var searchRequest = CreateSearchRequest(testData);
            var cancellationToken = TestContext.Current.CancellationToken;
            var expectedUri = new SongLyricsUriConverter().GetLyricUri(testData.ArtistName, testData.SongName);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.SongLyrics, searchResult.ExternalProviderType);
            Assert.Equal(testData.LyricResultData.Replace("\r\n", "\n"), searchResult.LyricText.Replace("\r\n", "\n"));
            Assert.False(searchResult.Instrumental);

            A.CallTo(() => webClient.LoadAsync(expectedUri, cancellationToken)).MustHaveHappenedOnceExactly();
            A.CallTo(() => webClient.Load(A<Uri>._, A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Theory]
        [MemberData(nameof(GetTestData), arguments: "Providers\\SongLyrics\\not_exist_test_data.json")]
        public void SearchLyric_LyricNotExistOnSite_ReturnsNoDataFound(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new SongLyricsProvider();
            lyricsClient.ConfigureExternalProvider(testData);

            var searchRequest = CreateSearchRequest(testData);

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.SongLyrics, searchResult.ExternalProviderType);
            Assert.False(searchResult.Instrumental);
        }

        [Theory]
        [MemberData(nameof(GetTestData), arguments: "Providers\\SongLyrics\\lyric_test_data.json")]
        public void SearchLyric_UnitDynamicData_Success(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new SongLyricsProvider();
            lyricsClient.ConfigureExternalProvider(testData);

            SearchRequest searchRequest = CreateSearchRequest(testData);
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.SongLyrics, searchResult.ExternalProviderType);
            Assert.Equal(testData.LyricResultData.Replace("\r\n", "\n"), searchResult.LyricText.Replace("\r\n", "\n"));
            Assert.False(searchResult.Instrumental);
        }

        [Theory]
        [MemberData(nameof(GetTestData), arguments: "Providers\\SongLyrics\\instrumental_test_data.json")]
        public void SearchLyric_UnitDynamicData_Instrumental(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new SongLyricsProvider();
            lyricsClient.ConfigureExternalProvider(testData);

            SearchRequest searchRequest = CreateSearchRequest(testData);
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.SongLyrics, searchResult.ExternalProviderType);
            Assert.True(searchResult.Instrumental);
        }
    }
}
