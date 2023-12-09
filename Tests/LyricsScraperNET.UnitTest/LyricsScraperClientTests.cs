using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Providers.Models;
using Moq;
using Xunit;

namespace LyricsScraperNET.UnitTest
{
    public class LyricsScraperClientTests
    {
        [Fact]
        public async void SearchLyric_WithDisabledClient_ShouldReturnEmptySearchResult()
        {
            // Arrange
            var lyricsScraperClient = GetLyricsScraperClient();
            var searchRequestMock = new Mock<SearchRequest>();
            var externalProviderTypes = GetExternalProviderTypes();

            // Act
            lyricsScraperClient.Disable();
            var searchResult = lyricsScraperClient.SearchLyric(searchRequestMock.Object);
            var searchResultAsync = await lyricsScraperClient.SearchLyricAsync(searchRequestMock.Object);

            // Assert
            Assert.False(lyricsScraperClient.IsEnabled);
            foreach (var providerType in externalProviderTypes)
            {
                Assert.False(lyricsScraperClient[providerType].IsEnabled);
            }
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.NotNull(searchResultAsync);
            Assert.True(searchResultAsync.IsEmpty());
        }

        [Fact]
        public async void SearchLyric_DefaultClient_ShouldReturnEmptySearchResult()
        {
            // Arrange
            var lyricsScraperClient = new LyricsScraperClient();
            var searchRequestMock = new Mock<SearchRequest>();

            // Act
            var searchResult = lyricsScraperClient.SearchLyric(searchRequestMock.Object);
            var searchResultAsync = await lyricsScraperClient.SearchLyricAsync(searchRequestMock.Object);

            // Assert
            Assert.False(lyricsScraperClient.IsEnabled);
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.NotNull(searchResultAsync);
            Assert.True(searchResultAsync.IsEmpty());
        }

        [Fact]
        public void Indexer_DefaultClient_ShouldReturnEmptyExternalProvider()
        {
            // Arrange
            var lyricsScraperClient = new LyricsScraperClient();

            // Act
            var externalProvider = lyricsScraperClient[ExternalProviderType.Genius];

            // Assert
            Assert.Null(externalProvider);
        }

        [Fact]
        public void Indexer_ConfiguredClient_ShouldReturnEmptyExternalProvider()
        {
            // Arrange
            var lyricsScraperClient = GetLyricsScraperClient();

            // Act
            var externalProvider = lyricsScraperClient[ExternalProviderType.Genius];

            // Assert
            Assert.Null(externalProvider);
        }

        [Fact]
        public async void Enable_WithDisabledClient_ShouldBeEnabled()
        {
            // Arrange
            var lyricsScraperClient = GetLyricsScraperClient();
            var searchRequestMock = new Mock<SearchRequest>();
            var externalProviderTypes = GetExternalProviderTypes();

            // Act
            lyricsScraperClient.Disable();
            lyricsScraperClient.Enable();

            // Assert
            Assert.True(lyricsScraperClient.IsEnabled);
            foreach (var providerType in externalProviderTypes)
            {
                Assert.True(lyricsScraperClient[providerType].IsEnabled);
            }
        }

        private ExternalProviderType[] GetExternalProviderTypes()
        {
            return new[] { ExternalProviderType.AZLyrics, ExternalProviderType.SongLyrics };
        }

        private ILyricsScraperClient GetLyricsScraperClient()
        {
            return new LyricsScraperClient()
                .WithAZLyrics()
                .WithSongLyrics();
        }
    }
}
