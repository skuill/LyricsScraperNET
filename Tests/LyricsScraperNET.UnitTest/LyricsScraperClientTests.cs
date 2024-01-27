using LyricsScraperNET.Common;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;
using Moq;
using System;
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
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.Equal(Constants.ResponseMessages.ExternalProvidersAreDisabled, searchResult.ResponseMessage);
            Assert.False(searchResult.Instrumental);
            Assert.NotNull(searchResultAsync);
            Assert.True(searchResultAsync.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResultAsync.ResponseStatusCode);
            Assert.Equal(Constants.ResponseMessages.ExternalProvidersAreDisabled, searchResultAsync.ResponseMessage);
            Assert.False(searchResultAsync.Instrumental);
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
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.Equal(Constants.ResponseMessages.ExternalProvidersListIsEmpty, searchResult.ResponseMessage);
            Assert.False(searchResult.Instrumental);
            Assert.NotNull(searchResultAsync);
            Assert.True(searchResultAsync.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResultAsync.ResponseStatusCode);
            Assert.Equal(Constants.ResponseMessages.ExternalProvidersListIsEmpty, searchResultAsync.ResponseMessage);
            Assert.False(searchResultAsync.Instrumental);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(null, null)]
        [InlineData("Muse", null)]
        [InlineData(null, "Hysteria")]
        public async void SearchLyric_MalformedArtistAndSongSearchRequest_ShouldReturnBadRequestStatus(string artist, string song)
        {
            // Arrange
            var lyricsScraperClient = new LyricsScraperClient();
            var searchRequest = new ArtistAndSongSearchRequest(artist, song);

            // Act
            var searchResult = lyricsScraperClient.SearchLyric(searchRequest);
            var searchResultAsync = await lyricsScraperClient.SearchLyricAsync(searchRequest);

            // Assert
            Assert.False(lyricsScraperClient.IsEnabled);
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ExternalProviderType.None, searchResult.ExternalProviderType);
            Assert.Equal(ResponseStatusCode.BadRequest, searchResult.ResponseStatusCode);
            Assert.Equal(Constants.ResponseMessages.ArtistAndSongSearchRequestFieldsAreEmpty, searchResult.ResponseMessage);
            Assert.False(searchResult.Instrumental);
            Assert.NotNull(searchResultAsync);
            Assert.True(searchResultAsync.IsEmpty());
            Assert.Equal(ExternalProviderType.None, searchResultAsync.ExternalProviderType);
            Assert.Equal(ResponseStatusCode.BadRequest, searchResultAsync.ResponseStatusCode);
            Assert.Equal(Constants.ResponseMessages.ArtistAndSongSearchRequestFieldsAreEmpty, searchResultAsync.ResponseMessage);
            Assert.False(searchResultAsync.Instrumental);
        }

        [Theory]
        [InlineData(null)]
        public async void SearchLyric_MalformedUriSearchRequest_ShouldReturnBadRequestStatus(Uri uri)
        {
            // Arrange
            var lyricsScraperClient = new LyricsScraperClient();
            var searchRequest = new UriSearchRequest(uri);

            // Act
            var searchResult = lyricsScraperClient.SearchLyric(searchRequest);
            var searchResultAsync = await lyricsScraperClient.SearchLyricAsync(searchRequest);

            // Assert
            Assert.False(lyricsScraperClient.IsEnabled);
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ExternalProviderType.None, searchResult.ExternalProviderType);
            Assert.Equal(ResponseStatusCode.BadRequest, searchResult.ResponseStatusCode);
            Assert.Equal(Constants.ResponseMessages.UriSearchRequestFieldsAreEmpty, searchResult.ResponseMessage);
            Assert.False(searchResult.Instrumental);
            Assert.NotNull(searchResultAsync);
            Assert.True(searchResultAsync.IsEmpty());
            Assert.Equal(ExternalProviderType.None, searchResultAsync.ExternalProviderType);
            Assert.Equal(ResponseStatusCode.BadRequest, searchResultAsync.ResponseStatusCode);
            Assert.Equal(Constants.ResponseMessages.UriSearchRequestFieldsAreEmpty, searchResultAsync.ResponseMessage);
            Assert.False(searchResultAsync.Instrumental);
        }

        [Fact]
        public async void SearchLyric_EmptySearchRequest_ShouldReturnBadRequestStatus()
        {
            // Arrange
            var lyricsScraperClient = new LyricsScraperClient();

            // Act
            var searchResult = lyricsScraperClient.SearchLyric(null);
            var searchResultAsync = await lyricsScraperClient.SearchLyricAsync(null);

            // Assert
            Assert.False(lyricsScraperClient.IsEnabled);
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ExternalProviderType.None, searchResult.ExternalProviderType);
            Assert.Equal(ResponseStatusCode.BadRequest, searchResult.ResponseStatusCode);
            Assert.Equal(Constants.ResponseMessages.SearchRequestIsEmpty, searchResult.ResponseMessage);
            Assert.False(searchResult.Instrumental);
            Assert.NotNull(searchResultAsync);
            Assert.True(searchResultAsync.IsEmpty());
            Assert.Equal(ExternalProviderType.None, searchResultAsync.ExternalProviderType);
            Assert.Equal(ResponseStatusCode.BadRequest, searchResultAsync.ResponseStatusCode);
            Assert.Equal(Constants.ResponseMessages.SearchRequestIsEmpty, searchResultAsync.ResponseMessage);
            Assert.False(searchResultAsync.Instrumental);
        }

        [Fact]
        public async void SearchLyric_ProviderWithEmptyResult_ShouldReturnNotFoundStatus()
        {
            // Arrange
            var lyricsScraperClient = GetLyricsScraperClientWithMockedProvider();
            var searchRequestMock = new Mock<SearchRequest>();

            // Act
            var searchResult = lyricsScraperClient.SearchLyric(searchRequestMock.Object);
            var searchResultAsync = await lyricsScraperClient.SearchLyricAsync(searchRequestMock.Object);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ExternalProviderType.None, searchResult.ExternalProviderType);
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.Equal(Constants.ResponseMessages.NotFoundLyric, searchResult.ResponseMessage);
            Assert.False(searchResult.Instrumental);
            Assert.NotNull(searchResultAsync);
            Assert.True(searchResultAsync.IsEmpty());
            Assert.Equal(ExternalProviderType.None, searchResultAsync.ExternalProviderType);
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResultAsync.ResponseStatusCode);
            Assert.Equal(Constants.ResponseMessages.NotFoundLyric, searchResultAsync.ResponseMessage);
            Assert.False(searchResultAsync.Instrumental);
        }

        [Fact]
        public async void SearchLyric_ProviderNotSpecifiedForRequest_ShouldReturnNotFoundStatus()
        {
            // Arrange
            var lyricsScraperClient = GetLyricsScraperClientWithMockedProvider();
            var searchRequest = new ArtistAndSongSearchRequest("test", "test", ExternalProviderType.Genius);

            // Act
            var searchResult = lyricsScraperClient.SearchLyric(searchRequest);
            var searchResultAsync = await lyricsScraperClient.SearchLyricAsync(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ExternalProviderType.None, searchResult.ExternalProviderType);
            Assert.Equal(ResponseStatusCode.BadRequest, searchResult.ResponseStatusCode);
            Assert.Equal(Constants.ResponseMessages.ExternalProviderForRequestNotSpecified, searchResult.ResponseMessage);
            Assert.False(searchResult.Instrumental);
            Assert.NotNull(searchResultAsync);
            Assert.True(searchResultAsync.IsEmpty());
            Assert.Equal(ExternalProviderType.None, searchResultAsync.ExternalProviderType);
            Assert.Equal(ResponseStatusCode.BadRequest, searchResultAsync.ResponseStatusCode);
            Assert.Equal(Constants.ResponseMessages.ExternalProviderForRequestNotSpecified, searchResultAsync.ResponseMessage);
            Assert.False(searchResultAsync.Instrumental);
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
            Assert.False(lyricsScraperClient.IsEnabled);

            lyricsScraperClient.Enable();

            // Assert
            Assert.True(lyricsScraperClient.IsEnabled);
            foreach (var providerType in externalProviderTypes)
            {
                Assert.True(lyricsScraperClient[providerType].IsEnabled);
            }
        }

        [Fact]
        public void AddAndRemoveExternalProvider_ShouldBeSwitched()
        {
            // Arrange
            var lyricsScraperClient = new LyricsScraperClient();
            var externalProviderType = ExternalProviderType.AZLyrics;
            var externalProvider = GetExternalProviderMock(externalProviderType);

            // Assert 0
            Assert.Null(lyricsScraperClient[externalProviderType]);

            // Act 1
            lyricsScraperClient.AddProvider(externalProvider.Object);
            var externalProviderActual = lyricsScraperClient[externalProviderType];

            // Assert 1
            Assert.NotNull(externalProviderActual);

            // Act 2
            lyricsScraperClient.RemoveProvider(externalProviderType);

            // Assert 2
            Assert.Null(lyricsScraperClient[externalProviderType]);
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

        private ILyricsScraperClient GetLyricsScraperClientWithMockedProvider()
        {

            var client = new LyricsScraperClient();
            var externalProvider = GetExternalProviderMock(ExternalProviderType.AZLyrics);
            client.AddProvider(externalProvider.Object);

            return client;
        }

        private Mock<IExternalProvider> GetExternalProviderMock(ExternalProviderType externalProviderType)
        {
            var externalProviderMock = new Mock<IExternalProvider>();

            externalProviderMock.Setup(p => p.IsEnabled).Returns(true);
            externalProviderMock.Setup(p => p.SearchLyric(It.IsAny<SearchRequest>())).Returns(new SearchResult());
            externalProviderMock.Setup(p => p.SearchLyricAsync(It.IsAny<SearchRequest>())).ReturnsAsync(new SearchResult());
            externalProviderMock.Setup(p => p.Options.ExternalProviderType).Returns(externalProviderType);

            return externalProviderMock;
        }
    }
}
