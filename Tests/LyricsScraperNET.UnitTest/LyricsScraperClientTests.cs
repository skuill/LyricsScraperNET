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
using System.Diagnostics;
using System.Linq;
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
            Assert.Equal(cancellationTokenSource.Token, exception.CancellationToken);
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
            var result = await client.SearchLyricAsync(searchRequest, TestContext.Current.CancellationToken);

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
            var client = new LyricsScraperClient(configuration, mockedProviders)
            {
                UseParallelSearch = variableValue
            };

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
                .ReturnsLazily(async (SearchRequest r, CancellationToken ct) =>
                {
                    await Task.Delay(1000, ct); // Simulate a fast execution.
                    return fastResult;
                });
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
            var result = await client.SearchLyricAsync(searchRequest, TestContext.Current.CancellationToken);

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
            var result = await client.SearchLyricAsync(searchRequest, TestContext.Current.CancellationToken);

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
                .ReturnsLazily(async (SearchRequest r, CancellationToken ct) =>
                {
                    await Task.Delay(1000, ct); // Simulate a fast execution.
                    return fastResult;
                });
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
            var result = await client.SearchLyricAsync(searchRequest, TestContext.Current.CancellationToken);

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
            A.CallTo(() => slowProvider1.SearchPriority).Returns(1);

            // Add all providers to a list.
            var providers = new List<IExternalProvider> { slowProvider1, fastProvider };

            // Mock the configuration to enable parallel search.
            var config = A.Fake<ILyricScraperClientConfig>();
            A.CallTo(() => config.UseParallelSearch).Returns(false);

            // Create the client with the mocked dependencies.
            var client = new LyricsScraperClient(config, providers);

            // Act
            var result = await client.SearchLyricAsync(searchRequest, TestContext.Current.CancellationToken);

            // Assert
            // Verify that the result comes from the slow provider.
            Assert.Equal("Slow result", result.LyricText);

            // Ensure that the fast provider threw an exception.
            A.CallTo(() => fastProvider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();

            // Ensure that the slow provider was called exactly once.
            A.CallTo(() => slowProvider1.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SearchLyricAsync_ParallelSearch_HigherPriorityProviderWinsOverFasterProvider()
        {
            // Arrange
            var highPriorityResult = new SearchResult("High priority result", ExternalProviderType.Genius);
            var lowPriorityResult = new SearchResult("Low priority result", ExternalProviderType.AZLyrics);

            var highPriorityProvider = CreateDelayedProviderMock(priority: 10, delayMilliseconds: 300, highPriorityResult);
            var lowPriorityProvider = CreateDelayedProviderMock(priority: 1, delayMilliseconds: 0, lowPriorityResult);

            var client = CreateParallelClient(lowPriorityProvider, highPriorityProvider);
            var searchRequest = new ArtistAndSongSearchRequest("some artist", "some song");

            // Act
            var result = await client.SearchLyricAsync(searchRequest, TestContext.Current.CancellationToken);

            // Assert
            AssertProviderResult(highPriorityResult, result);
            A.CallTo(() => highPriorityProvider.SearchLyricAsync(searchRequest, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => lowPriorityProvider.SearchLyricAsync(searchRequest, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SearchLyricAsync_ParallelSearch_PriorityChangedDuringSearch_UsesPriorityFromSearchStart()
        {
            // Arrange
            // The high priority provider drops its priority as soon as the low priority provider is called,
            // like options changed by another thread while the search runs.
            using var lowProviderCalled = new ManualResetEventSlim(false);
            var highPriorityResult = new SearchResult("High priority result", ExternalProviderType.Genius);
            var lowPriorityResult = new SearchResult("Low priority result", ExternalProviderType.AZLyrics);

            var highPriorityProvider = CreateDelayedProviderMock(priority: 10, delayMilliseconds: 200, highPriorityResult);
            A.CallTo(() => highPriorityProvider.SearchPriority).ReturnsLazily(() => lowProviderCalled.IsSet ? 0 : 10);

            var lowPriorityProvider = CreateDelayedProviderMock(priority: 1, delayMilliseconds: 0, lowPriorityResult);
            A.CallTo(() => lowPriorityProvider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .Invokes(() => lowProviderCalled.Set())
                .Returns(lowPriorityResult);

            var client = CreateParallelClient(lowPriorityProvider, highPriorityProvider);
            var searchRequest = new ArtistAndSongSearchRequest("some artist", "some song");

            // Act
            var result = await client.SearchLyricAsync(searchRequest, TestContext.Current.CancellationToken);

            // Assert
            AssertProviderResult(highPriorityResult, result);
            Assert.True(lowProviderCalled.IsSet);
            A.CallTo(() => highPriorityProvider.SearchLyricAsync(searchRequest, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => lowPriorityProvider.SearchLyricAsync(searchRequest, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SearchLyricAsync_ParallelSearch_HigherPriorityProviderNotFound_ReturnsLowerPriorityResult()
        {
            // Arrange
            var lowPriorityResult = new SearchResult("Low priority result", ExternalProviderType.AZLyrics);

            var highPriorityProvider = CreateDelayedProviderMock(priority: 10, delayMilliseconds: 200, new SearchResult(ExternalProviderType.Genius));
            var lowPriorityProvider = CreateDelayedProviderMock(priority: 1, delayMilliseconds: 0, lowPriorityResult);

            var client = CreateParallelClient(lowPriorityProvider, highPriorityProvider);
            var searchRequest = new ArtistAndSongSearchRequest("some artist", "some song");

            // Act
            var result = await client.SearchLyricAsync(searchRequest, TestContext.Current.CancellationToken);

            // Assert
            AssertProviderResult(lowPriorityResult, result);
            A.CallTo(() => highPriorityProvider.SearchLyricAsync(searchRequest, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => lowPriorityProvider.SearchLyricAsync(searchRequest, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SearchLyricAsync_CallerCancels_ThrowsEvenIfProviderIgnoresToken(bool useParallelSearch)
        {
            // Arrange
            // The provider only finishes when the test opens the gate, so the search can only return by honoring the cancellation.
            var gate = new TaskCompletionSource<SearchResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            var provider = A.Fake<IExternalProvider>();
            A.CallTo(() => provider.IsEnabled).Returns(true);
            A.CallTo(() => provider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._)).Returns(gate.Task);

            var client = CreateClient(useParallelSearch, provider);
            var searchRequest = new ArtistAndSongSearchRequest("some artist", "some song");
            using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            cancellationTokenSource.CancelAfter(100);

            try
            {
                // Act
                var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                    client.SearchLyricAsync(searchRequest, cancellationTokenSource.Token).WaitAsync(SafetyTimeout, TestContext.Current.CancellationToken));

                // Assert
                Assert.Equal(cancellationTokenSource.Token, exception.CancellationToken);
                Assert.False(gate.Task.IsCompleted);
                A.CallTo(() => provider.SearchLyricAsync(searchRequest, A<CancellationToken>.That.Matches(ct => ct.IsCancellationRequested)))
                    .MustHaveHappenedOnceExactly();
            }
            finally
            {
                gate.TrySetResult(new SearchResult("Late result", ExternalProviderType.Genius));
            }
        }

        [Fact]
        public async Task SearchLyricAsync_ParallelSearch_AlreadyCanceledToken_ThrowsWithoutCallingProviders()
        {
            // Arrange
            var provider = CreateDelayedProviderMock(priority: 1, delayMilliseconds: 0, new SearchResult("Result", ExternalProviderType.Genius));
            var client = CreateParallelClient(provider);
            var searchRequest = new ArtistAndSongSearchRequest("some artist", "some song");
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act
            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                client.SearchLyricAsync(searchRequest, cancellationTokenSource.Token));

            // Assert
            Assert.Equal(cancellationTokenSource.Token, exception.CancellationToken);
            A.CallTo(() => provider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._)).MustNotHaveHappened();
            A.CallTo(() => provider.SearchLyric(A<SearchRequest>._, A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public void SearchLyric_ParallelSearch_UsesAsyncProviderMethods()
        {
            // Arrange
            var expectedResult = new SearchResult("Result", ExternalProviderType.Genius);
            var provider = CreateDelayedProviderMock(priority: 1, delayMilliseconds: 0, expectedResult);
            var client = CreateParallelClient(provider);
            var searchRequest = new ArtistAndSongSearchRequest("some artist", "some song");

            // Act
            var result = client.SearchLyric(searchRequest, TestContext.Current.CancellationToken);

            // Assert
            AssertProviderResult(expectedResult, result);
            A.CallTo(() => provider.SearchLyricAsync(searchRequest, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => provider.SearchLyric(A<SearchRequest>._, A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task SearchLyricAsync_ParallelSearch_NoProviderFound_ReturnsNotFound()
        {
            // Arrange
            var firstProvider = CreateDelayedProviderMock(priority: 2, delayMilliseconds: 50, new SearchResult(ExternalProviderType.Genius));
            var secondProvider = CreateDelayedProviderMock(priority: 1, delayMilliseconds: 0, new SearchResult(ExternalProviderType.AZLyrics));
            var client = CreateParallelClient(firstProvider, secondProvider);
            var searchRequest = new ArtistAndSongSearchRequest("some artist", "some song");

            // Act
            var result = await client.SearchLyricAsync(searchRequest, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, result.ResponseStatusCode);
            Assert.Equal(Constants.ResponseMessages.NotFoundLyric, result.ResponseMessage);
            Assert.Equal(ExternalProviderType.None, result.ExternalProviderType);
            Assert.False(result.Instrumental);
            A.CallTo(() => firstProvider.SearchLyricAsync(searchRequest, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => secondProvider.SearchLyricAsync(searchRequest, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        private static IExternalProvider CreateDelayedProviderMock(int priority, int delayMilliseconds, SearchResult result)
        {
            var provider = A.Fake<IExternalProvider>();
            A.CallTo(() => provider.IsEnabled).Returns(true);
            A.CallTo(() => provider.SearchPriority).Returns(priority);
            A.CallTo(() => provider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .ReturnsLazily(async (SearchRequest _, CancellationToken ct) =>
                {
                    if (delayMilliseconds > 0)
                        await Task.Delay(delayMilliseconds, ct);
                    return result;
                });
            return provider;
        }

        private static LyricsScraperClient CreateParallelClient(params IExternalProvider[] providers)
        {
            var config = A.Fake<ILyricScraperClientConfig>();
            A.CallTo(() => config.UseParallelSearch).Returns(true);
            return new LyricsScraperClient(config, providers);
        }

        private static LyricsScraperClient CreateClient(bool useParallelSearch, params IExternalProvider[] providers)
        {
            var config = A.Fake<ILyricScraperClientConfig>();
            A.CallTo(() => config.UseParallelSearch).Returns(useParallelSearch);
            return new LyricsScraperClient(config, providers);
        }

        /// <summary>
        /// Upper bound for operations that must finish much earlier: a broken timeout or cancellation fails the test instead of hanging it.
        /// </summary>
        private static readonly TimeSpan SafetyTimeout = TimeSpan.FromSeconds(10);

        private static List<string> GetLoggedMessages(ILogger logger, LogLevel logLevel)
            => Fake.GetCalls(logger)
                .Where(call => call.Method.Name == nameof(ILogger.Log) && call.GetArgument<LogLevel>(0) == logLevel)
                .Select(call => call.Arguments[2].ToString())
                .ToList();

        /// <summary>
        /// Provider whose search task fails after the client has stopped waiting for it. Keeps no reference to the task.
        /// </summary>
        private sealed class FailsLaterProvider : IExternalProvider
        {
            public static readonly TaskCompletionSource AllFailed = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly string _message;

            public FailsLaterProvider(string message) => _message = message;

            public IExternalProviderOptions Options { get; } = A.Fake<IExternalProviderOptions>();
            public bool IsEnabled => true;
            public int SearchPriority => 1;

            public SearchResult SearchLyric(SearchRequest searchRequest, CancellationToken cancellationToken = default)
                => throw new NotSupportedException();

            public Task<SearchResult> SearchLyricAsync(SearchRequest searchRequest, CancellationToken cancellationToken = default)
                => FailLater();

            private async Task<SearchResult> FailLater()
            {
                try
                {
                    await Task.Delay(300, CancellationToken.None);
                    throw new InvalidOperationException(_message);
                }
                finally
                {
                    AllFailed.TrySetResult();
                }
            }

            public void WithParser(IExternalProviderLyricParser parser) { }
            public void WithWebClient(LyricsScraperNET.Network.Abstract.IWebClient webClient) { }
            public void Enable() { }
            public void Disable() { }
            public void WithLogger(ILoggerFactory loggerFactory) { }
        }

        #endregion UseParallelSearch

        #region ProviderTimeout

        [Fact]
        public void ProviderTimeout_NotConfigured_ReturnsDefault()
        {
            // Arrange
            var client = new LyricsScraperClient();

            // Act
            var result = client.ProviderTimeout;

            // Assert
            Assert.Equal(TimeSpan.FromSeconds(10), result);
        }

        [Theory]
        [InlineData(null, null, 10000)]
        [InlineData(3000, null, 3000)]
        [InlineData(3000, 500, 500)]
        [InlineData(-5, null, 10000)]
        [InlineData(0, null, 10000)]
        public void ProviderTimeout_ConfigAndExplicitValue_ExplicitValueWinsInvalidConfigFallsBackToDefault(
            int? configMilliseconds,
            int? explicitMilliseconds,
            int expectedMilliseconds)
        {
            // Arrange
            var config = new LyricScraperClientConfig
            {
                ProviderTimeout = configMilliseconds.HasValue ? TimeSpan.FromMilliseconds(configMilliseconds.Value) : null
            };
            var client = new LyricsScraperClient(config, new[] { GetExternalProviderMock(ExternalProviderType.None) });
            if (explicitMilliseconds.HasValue)
                client.ProviderTimeout = TimeSpan.FromMilliseconds(explicitMilliseconds.Value);

            // Act
            var result = client.ProviderTimeout;

            // Assert
            Assert.Equal(TimeSpan.FromMilliseconds(expectedMilliseconds), result);
        }

        [Fact]
        public void ProviderTimeout_InfiniteTimeSpan_IsAllowed()
        {
            // Arrange
            var client = new LyricsScraperClient();

            // Act
            client.ProviderTimeout = Timeout.InfiniteTimeSpan;

            // Assert
            Assert.Equal(Timeout.InfiniteTimeSpan, client.ProviderTimeout);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void ProviderTimeout_NotPositive_Throws(int milliseconds)
        {
            // Arrange
            var client = new LyricsScraperClient();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => client.ProviderTimeout = TimeSpan.FromMilliseconds(milliseconds));
            Assert.Equal(TimeSpan.FromSeconds(10), client.ProviderTimeout);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SearchLyricAsync_ProviderDoesNotAnswerInTime_ReturnsResultOfNextProviderAndLogsWarning(bool useParallelSearch)
        {
            // Arrange
            // The hanging provider ignores its token and only finishes when the test opens the gate.
            var gate = new TaskCompletionSource<SearchResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            var hangingProvider = A.Fake<IExternalProvider>();
            A.CallTo(() => hangingProvider.IsEnabled).Returns(true);
            A.CallTo(() => hangingProvider.SearchPriority).Returns(10);
            A.CallTo(() => hangingProvider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._)).Returns(gate.Task);

            var expectedResult = new SearchResult("Result", ExternalProviderType.AZLyrics);
            var workingProvider = CreateDelayedProviderMock(priority: 1, delayMilliseconds: 0, expectedResult);

            var config = A.Fake<ILyricScraperClientConfig>();
            A.CallTo(() => config.UseParallelSearch).Returns(useParallelSearch);
            var logger = A.Fake<ILogger<LyricsScraperClient>>();
            var client = new LyricsScraperClient(logger, config, new[] { hangingProvider, workingProvider })
            {
                ProviderTimeout = TimeSpan.FromMilliseconds(200)
            };
            var searchRequest = new ArtistAndSongSearchRequest("some artist", "some song");
            var expectedWarning = $"Provider {hangingProvider.GetType().Name} did not answer within {TimeSpan.FromMilliseconds(200)}.";

            try
            {
                // Act
                var result = await client.SearchLyricAsync(searchRequest, TestContext.Current.CancellationToken).WaitAsync(SafetyTimeout, TestContext.Current.CancellationToken);

                // Assert
                AssertProviderResult(expectedResult, result);
                Assert.False(gate.Task.IsCompleted);
                A.CallTo(() => hangingProvider.SearchLyricAsync(searchRequest, A<CancellationToken>.That.Matches(ct => ct.IsCancellationRequested)))
                    .MustHaveHappenedOnceExactly();
                A.CallTo(() => workingProvider.SearchLyricAsync(searchRequest, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
                Assert.Equal(new[] { expectedWarning }, GetLoggedMessages(logger, LogLevel.Warning));
                Assert.Empty(GetLoggedMessages(logger, LogLevel.Error));
            }
            finally
            {
                gate.TrySetResult(new SearchResult("Late result", ExternalProviderType.Genius));
            }
        }

        [Fact]
        public async Task SearchLyricAsync_ProviderHonorsToken_TimeoutCancelsProviderToken()
        {
            // Arrange
            var observedToken = CancellationToken.None;
            var provider = A.Fake<IExternalProvider>();
            A.CallTo(() => provider.IsEnabled).Returns(true);
            A.CallTo(() => provider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .ReturnsLazily(async (SearchRequest _, CancellationToken ct) =>
                {
                    observedToken = ct;
                    await Task.Delay(5000, ct);
                    return new SearchResult("Late result", ExternalProviderType.Genius);
                });

            var client = new LyricsScraperClient(A.Fake<ILyricScraperClientConfig>(), new[] { provider })
            {
                ProviderTimeout = TimeSpan.FromMilliseconds(100)
            };
            var searchRequest = new ArtistAndSongSearchRequest("some artist", "some song");

            // Act
            var result = await client.SearchLyricAsync(searchRequest, TestContext.Current.CancellationToken);

            // Assert
            // The only provider did not answer: nobody could check the request.
            Assert.True(result.IsEmpty());
            Assert.Equal(SearchResultCode.AllProvidersFailed, result.Code);
            Assert.Equal(ResponseStatusCode.Error, result.ResponseStatusCode);
            Assert.Equal(Constants.ResponseMessages.AllProvidersFailed, result.ResponseMessage);
            var attempt = Assert.Single(result.ProviderAttempts);
            Assert.Equal(ProviderAttemptOutcome.Timeout, attempt.Outcome);
            Assert.Equal($"No answer within {client.ProviderTimeout}", attempt.Message);
            Assert.True(observedToken.IsCancellationRequested);
            A.CallTo(() => provider.SearchLyricAsync(searchRequest, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SearchLyricAsync_ProviderBlocksSynchronouslyBeforeReturningTask_TimeoutStillApplies(bool useParallelSearch)
        {
            // Arrange
            // The provider blocks the calling thread (ignoring its token) until the test opens the gate.
            using var gate = new ManualResetEventSlim(false);
            var blockingProvider = A.Fake<IExternalProvider>();
            A.CallTo(() => blockingProvider.IsEnabled).Returns(true);
            A.CallTo(() => blockingProvider.SearchPriority).Returns(10);
            A.CallTo(() => blockingProvider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .Invokes(() => gate.Wait(SafetyTimeout))
                .Returns(Task.FromResult(new SearchResult("Late result", ExternalProviderType.Genius)));

            var expectedResult = new SearchResult("Result", ExternalProviderType.AZLyrics);
            var workingProvider = CreateDelayedProviderMock(priority: 1, delayMilliseconds: 0, expectedResult);

            var client = CreateClient(useParallelSearch, blockingProvider, workingProvider);
            client.ProviderTimeout = TimeSpan.FromMilliseconds(200);
            var searchRequest = new ArtistAndSongSearchRequest("some artist", "some song");

            try
            {
                // Act
                var result = await client.SearchLyricAsync(searchRequest, TestContext.Current.CancellationToken).WaitAsync(SafetyTimeout, TestContext.Current.CancellationToken);

                // Assert
                AssertProviderResult(expectedResult, result);
                Assert.False(gate.IsSet);
                A.CallTo(() => blockingProvider.SearchLyricAsync(searchRequest, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
                A.CallTo(() => workingProvider.SearchLyricAsync(searchRequest, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            }
            finally
            {
                gate.Set();
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SearchLyricAsync_ProviderThrowsSynchronously_LogsErrorAndReturnsNextProviderResult(bool useParallelSearch)
        {
            // Arrange
            var providerException = new InvalidOperationException("Provider failed before returning a task");
            var throwingProvider = A.Fake<IExternalProvider>();
            A.CallTo(() => throwingProvider.IsEnabled).Returns(true);
            A.CallTo(() => throwingProvider.SearchPriority).Returns(10);
            A.CallTo(() => throwingProvider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._)).Throws(providerException);

            var expectedResult = new SearchResult("Result", ExternalProviderType.AZLyrics);
            var workingProvider = CreateDelayedProviderMock(priority: 1, delayMilliseconds: 0, expectedResult);

            var config = A.Fake<ILyricScraperClientConfig>();
            A.CallTo(() => config.UseParallelSearch).Returns(useParallelSearch);
            var logger = A.Fake<ILogger<LyricsScraperClient>>();
            var client = new LyricsScraperClient(logger, config, new[] { throwingProvider, workingProvider });
            var searchRequest = new ArtistAndSongSearchRequest("some artist", "some song");

            // Act
            var result = await client.SearchLyricAsync(searchRequest, TestContext.Current.CancellationToken);

            // Assert
            AssertProviderResult(expectedResult, result);
            A.CallTo(() => throwingProvider.SearchLyricAsync(searchRequest, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => workingProvider.SearchLyricAsync(searchRequest, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(logger)
                .Where(call => call.Method.Name == nameof(ILogger.Log)
                    && call.GetArgument<LogLevel>(0) == LogLevel.Error
                    && ReferenceEquals(call.GetArgument<Exception>(3), providerException))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SearchLyricAsync_AbandonedProviderFailsLater_ExceptionIsObserved()
        {
            // Arrange
            var marker = $"Abandoned provider failure {Guid.NewGuid()}";
            var unobserved = new System.Collections.Concurrent.ConcurrentBag<Exception>();
            EventHandler<UnobservedTaskExceptionEventArgs> handler = (_, e) =>
            {
                foreach (var exception in e.Exception.Flatten().InnerExceptions.Where(x => x.Message == marker))
                    unobserved.Add(exception);
            };

            // A hand-written provider: a fake would keep a reference to the returned task, so it could never be collected.
            var client = new LyricsScraperClient(A.Fake<ILyricScraperClientConfig>(), new IExternalProvider[] { new FailsLaterProvider(marker) })
            {
                ProviderTimeout = TimeSpan.FromMilliseconds(100)
            };
            var searchRequest = new ArtistAndSongSearchRequest("some artist", "some song");

            TaskScheduler.UnobservedTaskException += handler;
            try
            {
                // Act
                var result = await client.SearchLyricAsync(searchRequest, TestContext.Current.CancellationToken);
                await FailsLaterProvider.AllFailed.Task.WaitAsync(SafetyTimeout, TestContext.Current.CancellationToken);
                await Task.Delay(100, TestContext.Current.CancellationToken);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // Assert
                Assert.Equal(SearchResultCode.AllProvidersFailed, result.Code);
                Assert.Equal(ResponseStatusCode.Error, result.ResponseStatusCode);
                Assert.Empty(unobserved);
            }
            finally
            {
                TaskScheduler.UnobservedTaskException -= handler;
            }
        }

        #endregion ProviderTimeout

        #region Concurrency

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SearchLyricAsync_ManyConcurrentSearchesOnOneClient_EachGetsOwnResult(bool useParallelSearch)
        {
            // Arrange
            const int searchCount = 100;
            var highPriorityProvider = CreateEchoProviderMock(priority: 10, ExternalProviderType.Genius, findArtist: artist => artist.EndsWith('0'));
            var lowPriorityProvider = CreateEchoProviderMock(priority: 1, ExternalProviderType.AZLyrics, findArtist: _ => true);

            var config = A.Fake<ILyricScraperClientConfig>();
            A.CallTo(() => config.UseParallelSearch).Returns(useParallelSearch);
            var client = new LyricsScraperClient(config, new[] { lowPriorityProvider, highPriorityProvider });

            var requests = Enumerable.Range(0, searchCount)
                .Select(i => new ArtistAndSongSearchRequest($"artist {i}", $"song {i}"))
                .ToList();

            // Act
            var results = await Task.WhenAll(requests.Select(request =>
                Task.Run(() => client.SearchLyricAsync(request, TestContext.Current.CancellationToken))));

            // Assert
            for (int i = 0; i < searchCount; i++)
            {
                var expectedProvider = requests[i].Artist.EndsWith('0') ? ExternalProviderType.Genius : ExternalProviderType.AZLyrics;
                Assert.Equal($"Result for {requests[i].Artist} from {expectedProvider}", results[i].LyricText);
                Assert.Equal(expectedProvider, results[i].ExternalProviderType);
                Assert.Equal(ResponseStatusCode.Success, results[i].ResponseStatusCode);
            }

            A.CallTo(() => highPriorityProvider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._)).MustHaveHappened(searchCount, Times.Exactly);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void SearchLyric_SyncApiFromManyDedicatedThreads_EachGetsOwnResult(bool useParallelSearch)
        {
            // Arrange
            // Dedicated threads, not thread pool threads: blocking pool threads with the sync API is the documented
            // anti-pattern (thread pool starvation), and this test checks thread safety, not pool growth speed.
            const int searchCount = 30;
            var provider = CreateEchoProviderMock(priority: 1, ExternalProviderType.Genius, findArtist: _ => true);
            var client = CreateClient(useParallelSearch, provider);

            var requests = Enumerable.Range(0, searchCount)
                .Select(i => new ArtistAndSongSearchRequest($"artist {i}", $"song {i}"))
                .ToArray();
            var results = new SearchResult[searchCount];
            var errors = new System.Collections.Concurrent.ConcurrentBag<Exception>();
            var threads = Enumerable.Range(0, searchCount)
                .Select(i => new Thread(() =>
                {
                    try { results[i] = client.SearchLyric(requests[i], TestContext.Current.CancellationToken); }
                    catch (Exception ex) { errors.Add(ex); }
                }))
                .ToList();

            // Act
            threads.ForEach(thread => thread.Start());
            threads.ForEach(thread => Assert.True(thread.Join(SafetyTimeout)));

            // Assert
            Assert.Empty(errors);
            for (int i = 0; i < searchCount; i++)
            {
                Assert.Equal($"Result for {requests[i].Artist} from {ExternalProviderType.Genius}", results[i].LyricText);
                Assert.Equal(ExternalProviderType.Genius, results[i].ExternalProviderType);
                Assert.Equal(ResponseStatusCode.Success, results[i].ResponseStatusCode);
            }

            // Parallel mode uses the async provider methods, sequential mode the sync ones.
            A.CallTo(() => provider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .MustHaveHappened(useParallelSearch ? searchCount : 0, Times.Exactly);
            A.CallTo(() => provider.SearchLyric(A<SearchRequest>._, A<CancellationToken>._))
                .MustHaveHappened(useParallelSearch ? 0 : searchCount, Times.Exactly);
        }

        [Fact]
        public void SearchLyric_SequentialSync_RunsProviderOnCallerThread()
        {
            // Arrange
            // The sync API in sequential mode must not use extra thread pool threads: one blocked thread per search.
            var providerThreadId = -1;
            var expectedResult = new SearchResult("Result", ExternalProviderType.Genius);
            var provider = A.Fake<IExternalProvider>();
            A.CallTo(() => provider.IsEnabled).Returns(true);
            A.CallTo(() => provider.SearchLyric(A<SearchRequest>._, A<CancellationToken>._))
                .ReturnsLazily(() =>
                {
                    providerThreadId = Environment.CurrentManagedThreadId;
                    return expectedResult;
                });

            var client = CreateClient(useParallelSearch: false, provider);
            var searchRequest = new ArtistAndSongSearchRequest("some artist", "some song");

            // Act
            var result = client.SearchLyric(searchRequest, TestContext.Current.CancellationToken);

            // Assert
            AssertProviderResult(expectedResult, result);
            Assert.Equal(Environment.CurrentManagedThreadId, providerThreadId);
            A.CallTo(() => provider.SearchLyric(searchRequest, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => provider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public void SearchLyric_SequentialSyncProviderHonorsToken_TimeoutCancelsItAndNextProviderIsUsed()
        {
            // Arrange
            var observedToken = CancellationToken.None;
            var slowProvider = A.Fake<IExternalProvider>();
            A.CallTo(() => slowProvider.IsEnabled).Returns(true);
            A.CallTo(() => slowProvider.SearchPriority).Returns(10);
            A.CallTo(() => slowProvider.SearchLyric(A<SearchRequest>._, A<CancellationToken>._))
                .ReturnsLazily((SearchRequest _, CancellationToken ct) =>
                {
                    observedToken = ct;
                    ct.WaitHandle.WaitOne(SafetyTimeout); // Sync work that stops when the token is canceled.
                    ct.ThrowIfCancellationRequested();
                    return new SearchResult("Late result", ExternalProviderType.Genius);
                });

            var expectedResult = new SearchResult("Result", ExternalProviderType.AZLyrics);
            var workingProvider = A.Fake<IExternalProvider>();
            A.CallTo(() => workingProvider.IsEnabled).Returns(true);
            A.CallTo(() => workingProvider.SearchPriority).Returns(1);
            A.CallTo(() => workingProvider.SearchLyric(A<SearchRequest>._, A<CancellationToken>._)).Returns(expectedResult);

            var client = CreateClient(useParallelSearch: false, slowProvider, workingProvider);
            client.ProviderTimeout = TimeSpan.FromMilliseconds(200);
            var searchRequest = new ArtistAndSongSearchRequest("some artist", "some song");

            // Act
            var result = client.SearchLyric(searchRequest, TestContext.Current.CancellationToken);

            // Assert
            AssertProviderResult(expectedResult, result);
            Assert.True(observedToken.IsCancellationRequested);
            A.CallTo(() => slowProvider.SearchLyric(searchRequest, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => workingProvider.SearchLyric(searchRequest, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => slowProvider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SearchLyricAsync_OneSearchCanceled_OtherConcurrentSearchesComplete(bool useParallelSearch)
        {
            // Arrange
            var provider = A.Fake<IExternalProvider>();
            A.CallTo(() => provider.IsEnabled).Returns(true);
            A.CallTo(() => provider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .ReturnsLazily(async (SearchRequest request, CancellationToken ct) =>
                {
                    await Task.Delay(300, ct);
                    return new SearchResult($"Result for {((ArtistAndSongSearchRequest)request).Artist}", ExternalProviderType.Genius);
                });

            var config = A.Fake<ILyricScraperClientConfig>();
            A.CallTo(() => config.UseParallelSearch).Returns(useParallelSearch);
            var client = new LyricsScraperClient(config, new[] { provider });

            using var canceledSearchCts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            var canceledSearch = client.SearchLyricAsync(new ArtistAndSongSearchRequest("canceled", "song"), canceledSearchCts.Token);
            var otherSearches = Enumerable.Range(0, 10)
                .Select(i => client.SearchLyricAsync(new ArtistAndSongSearchRequest($"artist {i}", "song"), TestContext.Current.CancellationToken))
                .ToArray();

            // Act
            canceledSearchCts.Cancel();
            var otherResults = await Task.WhenAll(otherSearches);

            // Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => canceledSearch);
            for (int i = 0; i < otherResults.Length; i++)
            {
                Assert.Equal($"Result for artist {i}", otherResults[i].LyricText);
                Assert.Equal(ResponseStatusCode.Success, otherResults[i].ResponseStatusCode);
            }
        }

        [Fact]
        public async Task SearchLyricAsync_ProvidersAddedAndRemovedDuringSearches_DoesNotThrow()
        {
            // Arrange
            var client = new LyricsScraperClient();
            client.AddProvider(CreateEchoProviderMock(priority: 1, ExternalProviderType.Genius, findArtist: _ => true));
            var cancellationToken = TestContext.Current.CancellationToken;

            var searches = Enumerable.Range(0, 200)
                .Select(i => Task.Run(() => client.SearchLyricAsync(new ArtistAndSongSearchRequest($"artist {i}", "song"), cancellationToken)))
                .ToArray();

            var changes = Task.Run(() =>
            {
                for (int i = 0; i < 200; i++)
                {
                    client.AddProvider(CreateEchoProviderMock(priority: 0, ExternalProviderType.AZLyrics, findArtist: _ => false));
                    client.RemoveProvider(ExternalProviderType.AZLyrics);
                }
            }, cancellationToken);

            // Act
            var exception = await Record.ExceptionAsync(() => Task.WhenAll(searches.Append(changes)));

            // Assert
            Assert.Null(exception);
            Assert.All(searches, search => Assert.Equal(ResponseStatusCode.Success, search.Result.ResponseStatusCode));
            Assert.Null(client[ExternalProviderType.AZLyrics]);
            Assert.NotNull(client[ExternalProviderType.Genius]);
        }

        /// <summary>
        /// Provider that answers after a small random delay with a result built from the request,
        /// so concurrent searches can check that they got their own result.
        /// </summary>
        private static IExternalProvider CreateEchoProviderMock(int priority, ExternalProviderType providerType, Func<string, bool> findArtist)
        {
            var provider = A.Fake<IExternalProvider>();
            A.CallTo(() => provider.IsEnabled).Returns(true);
            A.CallTo(() => provider.SearchPriority).Returns(priority);
            A.CallTo(() => provider.Options.ExternalProviderType).Returns(providerType);
            A.CallTo(() => provider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .ReturnsLazily(async (SearchRequest request, CancellationToken ct) =>
                {
                    await Task.Delay(Random.Shared.Next(1, 20), ct);
                    var artist = ((ArtistAndSongSearchRequest)request).Artist;
                    return findArtist(artist)
                        ? new SearchResult($"Result for {artist} from {providerType}", providerType)
                        : new SearchResult(providerType);
                });
            A.CallTo(() => provider.SearchLyric(A<SearchRequest>._, A<CancellationToken>._))
                .ReturnsLazily((SearchRequest request, CancellationToken ct) =>
                {
                    var artist = ((ArtistAndSongSearchRequest)request).Artist;
                    return findArtist(artist)
                        ? new SearchResult($"Result for {artist} from {providerType}", providerType)
                        : new SearchResult(providerType);
                });
            return provider;
        }

        #endregion Concurrency

        #region helpers

        private static ExternalProviderType[] GetExternalProviderTypes()
        {
            return [ExternalProviderType.AZLyrics, ExternalProviderType.SongLyrics];
        }

        private static ILyricsScraperClient GetLyricsScraperClient()
        {
            return new LyricsScraperClient()
                .WithAZLyrics()
                .WithSongLyrics();
        }

        private static LyricsScraperClient GetLyricsScraperClientWithMockedProvider()
        {
            var client = new LyricsScraperClient();
            var externalProvider = GetExternalProviderMock(ExternalProviderType.AZLyrics);
            client.AddProvider(externalProvider);

            return client;
        }

        private static IExternalProvider GetExternalProviderMock(ExternalProviderType externalProviderType)
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

        /// <summary>
        /// Any valid request: the providers in these tests are fakes that accept every request.
        /// </summary>
        private static SearchRequest GetSearchRequestMock()
            => new ArtistAndSongSearchRequest("Artist", "Song");

        /// <summary>
        /// The client returns a copy of the provider result with <see cref="SearchResult.ProviderAttempts"/> filled.
        /// </summary>
        private static void AssertProviderResult(SearchResult expected, SearchResult actual)
        {
            Assert.NotSame(expected, actual);
            Assert.Equal(expected.LyricText, actual.LyricText);
            Assert.Equal(expected.ExternalProviderType, actual.ExternalProviderType);
            Assert.Equal(expected.Code, actual.Code);
            Assert.Equal(expected.ResponseMessage, actual.ResponseMessage);
            Assert.NotEmpty(actual.ProviderAttempts);
        }

        #endregion helpers
    }
}
