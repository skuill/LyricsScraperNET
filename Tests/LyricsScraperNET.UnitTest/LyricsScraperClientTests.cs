using FakeItEasy;
using LyricsScraperNET.Common;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LyricsScraperNET.UnitTest
{
    public class LyricsScraperClientTests
    {
        [Fact]
        public async Task SearchLyric_WithDisabledClient_ShouldReturnEmptySearchResult()
        {
            // Arrange
            var lyricsScraperClient = GetLyricsScraperClient();
            var searchRequestMock = GetSearchRequestMock();
            var externalProviderTypes = GetExternalProviderTypes();

            // Act
            lyricsScraperClient.Disable();
            var searchResult = lyricsScraperClient.SearchLyric(searchRequestMock);
            var searchResultAsync = await lyricsScraperClient.SearchLyricAsync(searchRequestMock);

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
        public async Task SearchLyric_DefaultClient_ShouldReturnEmptySearchResult()
        {
            // Arrange
            var lyricsScraperClient = new LyricsScraperClient();
            var searchRequestMock = GetSearchRequestMock();

            // Act
            var searchResult = lyricsScraperClient.SearchLyric(searchRequestMock);
            var searchResultAsync = await lyricsScraperClient.SearchLyricAsync(searchRequestMock);

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
        public async Task SearchLyric_MalformedArtistAndSongSearchRequest_ShouldReturnBadRequestStatus(string artist, string song)
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
        public async Task SearchLyric_MalformedUriSearchRequest_ShouldReturnBadRequestStatus(Uri uri)
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
        public async Task SearchLyric_EmptySearchRequest_ShouldReturnBadRequestStatus()
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
        public async Task SearchLyric_ProviderWithEmptyResult_ShouldReturnNotFoundStatus()
        {
            // Arrange
            var lyricsScraperClient = GetLyricsScraperClientWithMockedProvider();
            var searchRequestMock = GetSearchRequestMock();

            // Act
            var searchResult = lyricsScraperClient.SearchLyric(searchRequestMock);
            var searchResultAsync = await lyricsScraperClient.SearchLyricAsync(searchRequestMock);

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
        public async Task SearchLyric_ProviderNotSpecifiedForRequest_ShouldReturnNotFoundStatus()
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
        public void Enable_WithDisabledClient_ShouldBeEnabled()
        {
            // Arrange
            var lyricsScraperClient = GetLyricsScraperClient();
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
            lyricsScraperClient.AddProvider(externalProvider);
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
            client.AddProvider(externalProvider);

            return client;
        }

        private IExternalProvider GetExternalProviderMock(ExternalProviderType externalProviderType)
        {
            var externalProviderMock = A.Fake<IExternalProvider>();

            A.CallTo(() => externalProviderMock.IsEnabled).Returns(true);
            A.CallTo(() => externalProviderMock.SearchLyric(A<SearchRequest>._)).Returns(new SearchResult());
            A.CallTo(() => externalProviderMock.SearchLyricAsync(A<SearchRequest>._)).Returns(new SearchResult());
            A.CallTo(() => externalProviderMock.Options.ExternalProviderType).Returns(externalProviderType);

            return externalProviderMock;
        }

        private SearchRequest GetSearchRequestMock()
        {
            var searchRequestMock = A.Fake<SearchRequest>();
            string error = string.Empty;
            A.CallTo(() => searchRequestMock.IsValid(out error)).Returns(true);
            return searchRequestMock;
        }
    }
}
