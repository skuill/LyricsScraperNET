using FakeItEasy;
using LyricsScraperNET.Common;
using LyricsScraperNET.Configuration;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
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
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            lyricsScraperClient.Disable();
            var searchResult = lyricsScraperClient.SearchLyric(searchRequestMock, cancellationToken);
            var searchResultAsync = await lyricsScraperClient.SearchLyricAsync(searchRequestMock, cancellationToken);

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
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsScraperClient.SearchLyric(searchRequestMock, cancellationToken);
            var searchResultAsync = await lyricsScraperClient.SearchLyricAsync(searchRequestMock, cancellationToken);

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

        // If there is NotImplementedException, then the WithLogger method needs to be implemented for the missing provider.
        [Fact]
        public async Task WithLogger_ClientWithAllProviders_ShouldNotThrowException()
        {
            // Arrange
            var lyricsScraperClient = new LyricsScraperClient()
                .WithAllProviders();
            var mockedLoggerFactory = A.Fake<ILoggerFactory>();

            // Act
            var exception = Record.Exception(() => lyricsScraperClient.WithLogger(mockedLoggerFactory));

            // Assert
            Assert.Null(exception);
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
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsScraperClient.SearchLyric(searchRequest, cancellationToken);
            var searchResultAsync = await lyricsScraperClient.SearchLyricAsync(searchRequest, cancellationToken);

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
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsScraperClient.SearchLyric(searchRequest, cancellationToken);
            var searchResultAsync = await lyricsScraperClient.SearchLyricAsync(searchRequest, cancellationToken);

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
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsScraperClient.SearchLyric(null, cancellationToken);
            var searchResultAsync = await lyricsScraperClient.SearchLyricAsync(null, cancellationToken);

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
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsScraperClient.SearchLyric(searchRequestMock, cancellationToken);
            var searchResultAsync = await lyricsScraperClient.SearchLyricAsync(searchRequestMock, cancellationToken);

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
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsScraperClient.SearchLyric(searchRequest, cancellationToken);
            var searchResultAsync = await lyricsScraperClient.SearchLyricAsync(searchRequest, cancellationToken);

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

        [Theory]
        [InlineData(ExternalProviderType.AZLyrics)]
        [InlineData(ExternalProviderType.None)]
        public void RemoveProvider_NotExisted_ShouldNotThrowException(ExternalProviderType externalProviderType)
        {
            // Arrange
            var lyricsScraperClient = new LyricsScraperClient();

            // Assert 0
            Assert.Null(lyricsScraperClient[externalProviderType]);

            // Act
            var exception = Record.Exception(() => lyricsScraperClient.RemoveProvider(externalProviderType));

            // Assert 1
            Assert.Null(exception);
        }

        [Fact]
        public void AddProvider_NullProvider_ShouldThrowException()
        {
            // Arrange
            var lyricsScraperClient = new LyricsScraperClient();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => lyricsScraperClient.AddProvider(null));
        }

        [Fact]
        public void SearchLyric_Should_Throw_OperationCanceledException_When_Cancellation_Is_Requested()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            var lyricsScraperClient = GetLyricsScraperClientWithMockedProvider();
            var searchRequest = GetSearchRequestMock();

            // Act
            var exception = Assert.ThrowsAny<OperationCanceledException>(() =>
                lyricsScraperClient.SearchLyric(searchRequest, cancellationTokenSource.Token));

            // Assert
            Assert.IsType<TaskCanceledException>(exception); // Check that this is TaskCanceledException
        }

        [Fact]
        public async Task SearchLyricAsync_Should_Throw_OperationCanceledException_When_Cancellation_Is_Requested()
        {
            // Arrange
            var searchRequest = GetSearchRequestMock();
            var client = GetLyricsScraperClientWithMockedProvider();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                client.SearchLyricAsync(searchRequest, cancellationTokenSource.Token));
        }

        [Fact]
        public async Task SearchLyricAsync_Should_Not_Cancel_If_Token_Not_Requested()
        {
            // Arrange
            var searchRequest = GetSearchRequestMock();
            var client = GetLyricsScraperClientWithMockedProvider();
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var result = await client.SearchLyricAsync(searchRequest, cancellationTokenSource.Token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchLyricAsync_Should_Not_Cancel_If_Token_Default()
        {
            // Arrange
            var searchRequest = GetSearchRequestMock();
            var client = GetLyricsScraperClientWithMockedProvider();

            // Act
            var result = await client.SearchLyricAsync(searchRequest);

            // Assert
            Assert.NotNull(result);
        }

        #region UseParallelSearch

        [Fact]
        public void UseParallelSearchEnabled_ShouldUseValueFromConfig()
        {
            // Arrange
            var configuration = new LyricScraperClientConfig { UseParallelSearch = true };
            var mockedProviders = new[] { GetExternalProviderMock(ExternalProviderType.None) };
            var client = new LyricsScraperClient(configuration, mockedProviders);

            // Act
            var result = client.UseParallelSearch;

            // Arrange
            Assert.True(result);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public void UseParallelSearchEnabled_ShouldUseLocalVariable_WhenExplicitlySet(
            bool configValue,
            bool variableValue,
            bool expectedResult)
        {
            // Arrange
            var configuration = new LyricScraperClientConfig { UseParallelSearch = configValue };
            var mockedProviders = new[] { GetExternalProviderMock(ExternalProviderType.None) };
            var client = new LyricsScraperClient(configuration, mockedProviders);
            client.UseParallelSearch = variableValue;

            // Act
            var actualResult = client.UseParallelSearch;

            // Arrange
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public async Task SearchLyricAsync_WithUseParallelSearchEnabled_ShouldReturnFirstResultAndCancelOthersInParallelMode()
        {
            // Arrange
            var searchRequest = new ArtistAndSongSearchRequest("some artist", "some song");

            // Create a fast provider that will return a result immediately.
            var fastResult = new SearchResult("Fast result", ExternalProviderType.None);

            var fastProvider = A.Fake<IExternalProvider>();
            A.CallTo(() => fastProvider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .Returns(fastResult);
            A.CallTo(() => fastProvider.IsEnabled).Returns(true);

            // Create slow providers that simulate delayed response.
            var slowProvider1 = A.Fake<IExternalProvider>();
            A.CallTo(() => slowProvider1.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .ReturnsLazily(async (SearchRequest r, CancellationToken ct) =>
                {
                    await Task.Delay(10000, ct); // Simulate a long delay.
                    return SearchResult.Empty;
                });
            A.CallTo(() => slowProvider1.IsEnabled).Returns(true);

            var slowProvider2 = A.Fake<IExternalProvider>();
            A.CallTo(() => slowProvider2.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .ReturnsLazily(async (SearchRequest r, CancellationToken ct) =>
                {
                    await Task.Delay(10000, ct); // Simulate a long delay.
                    return SearchResult.Empty;
                });
            A.CallTo(() => slowProvider2.IsEnabled).Returns(true);

            // Add all providers to a list.
            var providers = new List<IExternalProvider> { slowProvider1, slowProvider2, fastProvider };

            // Mock the configuration to enable parallel search.
            var config = A.Fake<ILyricScraperClientConfig>();
            A.CallTo(() => config.UseParallelSearch).Returns(true);

            // Create the client with the mocked dependencies.
            var client = new LyricsScraperClient(config, providers);

            // Act
            var result = await client.SearchLyricAsync(searchRequest);

            // Assert
            // Verify the result matches the fast provider's response.
            Assert.Equal("Fast result", result.LyricText);

            // Ensure all providers were called exactly once.
            A.CallTo(() => fastProvider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => slowProvider1.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => slowProvider2.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();

            // Ensure cancellation for slow providers (via cancellation token).
            A.CallTo(() => slowProvider1.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>.That.Matches(ct => ct.IsCancellationRequested)))
                .MustHaveHappened();
            A.CallTo(() => slowProvider2.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>.That.Matches(ct => ct.IsCancellationRequested)))
                .MustHaveHappened();

            // Verify that slow providers are not called again after cancellation
            A.CallTo(() => slowProvider1.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => slowProvider2.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SearchLyricAsync_WithUseParallelSearchEnabled_ShouldReturnResultFromSlowProvider_WhenFastProviderFails()
        {
            // Arrange
            var searchRequest = new ArtistAndSongSearchRequest("some artist", "some song");

            // Create a fast provider that will fail (return empty result).
            var fastResult = SearchResult.Empty;
            var fastProvider = A.Fake<IExternalProvider>();
            A.CallTo(() => fastProvider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .Returns(fastResult);
            A.CallTo(() => fastProvider.IsEnabled).Returns(true);

            // Create slow providers that simulate delayed response but return valid results.
            var slowResult = new SearchResult("Slow result", ExternalProviderType.None);

            var slowProvider1 = A.Fake<IExternalProvider>();
            A.CallTo(() => slowProvider1.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .ReturnsLazily(async (SearchRequest r, CancellationToken ct) =>
                {
                    await Task.Delay(2000, ct); // Simulate a long delay.
                    return slowResult;
                });
            A.CallTo(() => slowProvider1.IsEnabled).Returns(true);

            // Add all providers to a list.
            var providers = new List<IExternalProvider> { slowProvider1, fastProvider };

            // Mock the configuration to enable parallel search.
            var config = A.Fake<ILyricScraperClientConfig>();
            A.CallTo(() => config.UseParallelSearch).Returns(true);

            // Create the client with the mocked dependencies.
            var client = new LyricsScraperClient(config, providers);

            // Act
            var result = await client.SearchLyricAsync(searchRequest);

            // Assert
            Assert.Equal("Slow result", result.LyricText);

            // Ensure all providers were called exactly once.
            A.CallTo(() => fastProvider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => slowProvider1.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();

            // Ensure cancellation for slow providers.
            A.CallTo(() => slowProvider1.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>.That.Matches(ct => ct.IsCancellationRequested)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task SearchLyricAsync_WithUseParallelSearchEnabled_ShouldUseOnlyChildCancellationToken_WhenFastSearchIsExecuted()
        {
            // Arrange
            var searchRequest = new ArtistAndSongSearchRequest("some artist", "some song");

            // Create a fast provider that will return a result immediately.
            var fastResult = new SearchResult("Fast result", ExternalProviderType.None);
            var fastProvider = A.Fake<IExternalProvider>();
            A.CallTo(() => fastProvider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .Returns(fastResult);
            A.CallTo(() => fastProvider.IsEnabled).Returns(true);

            // Create slow providers that simulate delayed response but are not used because fast provider wins.
            var slowProvider1 = A.Fake<IExternalProvider>();
            A.CallTo(() => slowProvider1.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .ReturnsLazily(async (SearchRequest r, CancellationToken ct) =>
                {
                    await Task.Delay(10000, ct); // Simulate a long delay.
                    return SearchResult.Empty;
                });
            A.CallTo(() => slowProvider1.IsEnabled).Returns(true);

            // Add all providers to a list.
            var providers = new List<IExternalProvider> { slowProvider1, fastProvider };

            // Mock the configuration to enable parallel search.
            var config = A.Fake<ILyricScraperClientConfig>();
            A.CallTo(() => config.UseParallelSearch).Returns(true);

            // Create the client with the mocked dependencies.
            var client = new LyricsScraperClient(config, providers);

            // Create a parent cancellation token and a child token for the search.
            var parentCancellationTokenSource = new CancellationTokenSource();
            var childCancellationToken = parentCancellationTokenSource.Token;

            // Act
            var result = await client.SearchLyricAsync(searchRequest, childCancellationToken);

            // Assert
            Assert.Equal("Fast result", result.LyricText);

            // Ensure no cancellation occurred in the parent token.
            Assert.False(parentCancellationTokenSource.Token.IsCancellationRequested);

            // Ensure the fast provider was called once and slow providers were cancelled.
            A.CallTo(() => fastProvider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => slowProvider1.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>.That.Matches(ct => ct.IsCancellationRequested)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task SearchLyricAsync_WithUseParallelSearchEnabled_ShouldReturnResultFromSlowProvider_WhenFastProviderThrowsException()
        {
            // Arrange
            var searchRequest = new ArtistAndSongSearchRequest("some artist", "some song");

            // Create a fast provider that throws an exception.
            var fastProvider = A.Fake<IExternalProvider>();
            A.CallTo(() => fastProvider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .ThrowsAsync(new Exception("Fast provider failed"));
            A.CallTo(() => fastProvider.IsEnabled).Returns(true);

            // Create slow providers that simulate delayed response but return valid results.
            var slowResult = new SearchResult("Slow result", ExternalProviderType.None);

            var slowProvider1 = A.Fake<IExternalProvider>();
            A.CallTo(() => slowProvider1.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .ReturnsLazily(async (SearchRequest r, CancellationToken ct) =>
                {
                    await Task.Delay(2000, ct); // Simulate a long delay.
                    return slowResult;
                });
            A.CallTo(() => slowProvider1.IsEnabled).Returns(true);

            // Add all providers to a list.
            var providers = new List<IExternalProvider> { slowProvider1, fastProvider };

            // Mock the configuration to enable parallel search.
            var config = A.Fake<ILyricScraperClientConfig>();
            A.CallTo(() => config.UseParallelSearch).Returns(true);

            // Create the client with the mocked dependencies.
            var client = new LyricsScraperClient(config, providers);

            // Act
            var result = await client.SearchLyricAsync(searchRequest);

            // Assert
            // Verify that the result comes from the slow provider.
            Assert.Equal("Slow result", result.LyricText);

            // Ensure that the fast provider threw an exception.
            A.CallTo(() => fastProvider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();

            // Ensure that the slow provider was called exactly once.
            A.CallTo(() => slowProvider1.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }


        [Fact]
        public async Task SearchLyricAsync_WithUseParallelSearchDisabled_ShouldReturnResultFromFastProvider_WhenSlowProviderThrowsException()
        {
            // Arrange
            var searchRequest = new ArtistAndSongSearchRequest("some artist", "some song");

            // Create a fast provider that throws an exception.
            var fastProvider = A.Fake<IExternalProvider>();
            A.CallTo(() => fastProvider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .ThrowsAsync(new Exception("Fast provider failed"));
            A.CallTo(() => fastProvider.IsEnabled).Returns(true);
            A.CallTo(() => fastProvider.SearchPriority).Returns(1000);

            // Create slow providers that simulate delayed response but return valid results.
            var slowResult = new SearchResult("Slow result", ExternalProviderType.None);

            var slowProvider1 = A.Fake<IExternalProvider>();
            A.CallTo(() => slowProvider1.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .ReturnsLazily(async (SearchRequest r, CancellationToken ct) =>
                {
                    await Task.Delay(2000, ct); // Simulate a long delay.
                    return slowResult;
                });
            A.CallTo(() => slowProvider1.IsEnabled).Returns(true);
            A.CallTo(() => fastProvider.SearchPriority).Returns(1);

            // Add all providers to a list.
            var providers = new List<IExternalProvider> { slowProvider1, fastProvider };

            // Mock the configuration to enable parallel search.
            var config = A.Fake<ILyricScraperClientConfig>();
            A.CallTo(() => config.UseParallelSearch).Returns(false);

            // Create the client with the mocked dependencies.
            var client = new LyricsScraperClient(config, providers);

            // Act
            var result = await client.SearchLyricAsync(searchRequest);

            // Assert
            // Verify that the result comes from the slow provider.
            Assert.Equal("Slow result", result.LyricText);

            // Ensure that the fast provider threw an exception.
            A.CallTo(() => fastProvider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();

            // Ensure that the slow provider was called exactly once.
            A.CallTo(() => slowProvider1.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        #endregion UseParallelSearch

        #region helpers

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
            A.CallTo(() => externalProviderMock.SearchLyric(A<SearchRequest>._, A<CancellationToken>._))
                .ReturnsLazily((SearchRequest _, CancellationToken token) =>
                {
                    token.ThrowIfCancellationRequested();
                    return new SearchResult();
                });
            A.CallTo(() => externalProviderMock.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .ReturnsLazily(async (SearchRequest _, CancellationToken token) =>
                {
                    await Task.Delay(50, token); // Simulate some delay
                    token.ThrowIfCancellationRequested();
                    return new SearchResult();
                });
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

        #endregion helpers
    }
}
