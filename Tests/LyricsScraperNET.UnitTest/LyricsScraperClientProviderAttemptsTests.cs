using FakeItEasy;
using LyricsScraperNET.Common;
using LyricsScraperNET.Configuration;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.Providers.SongLyrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static LyricsScraperNET.UnitTest.Network.HttpTestHelper;

namespace LyricsScraperNET.UnitTest
{
    /// <summary>
    /// <see cref="SearchResult.Code"/> and <see cref="SearchResult.ProviderAttempts"/> filled by the client (#74).
    /// </summary>
    public class LyricsScraperClientProviderAttemptsTests
    {
        private static readonly ArtistAndSongSearchRequest Request = new("Artist", "Song");

        [Fact]
        public async Task SearchLyricAsync_SequentialFirstNotFoundThenFound_RecordsBothInOrder()
        {
            // Arrange
            var notFound = CreateProvider(ExternalProviderType.AZLyrics, priority: 2, _ => Task.FromResult(new SearchResult(ExternalProviderType.AZLyrics)));
            var found = CreateProvider(ExternalProviderType.Genius, priority: 1, _ => Task.FromResult(new SearchResult("Lyrics", ExternalProviderType.Genius)));
            var client = CreateClient(useParallelSearch: false, notFound, found);

            // Act
            var result = await client.SearchLyricAsync(Request, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal((SearchResultCode.Found, "Lyrics", ExternalProviderType.Genius), (result.Code, result.LyricText, result.ExternalProviderType));
            Assert.Equal(new[]
            {
                (ExternalProviderType.AZLyrics, ProviderAttemptOutcome.NotFound, string.Empty),
                (ExternalProviderType.Genius, ProviderAttemptOutcome.Found, string.Empty),
            }, Describe(result));
            Assert.All(result.ProviderAttempts, attempt => Assert.True(attempt.Duration >= TimeSpan.Zero));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SearchLyricAsync_ProviderThrowsAndOtherAnswersNotFound_ReturnsNotFoundWithErrorAttempt(bool useParallelSearch)
        {
            // Arrange
            var throwing = CreateProvider(ExternalProviderType.AZLyrics, priority: 2, _ => throw new InvalidOperationException("boom"));
            var notFound = CreateProvider(ExternalProviderType.Genius, priority: 1, _ => Task.FromResult(new SearchResult(ExternalProviderType.Genius)));
            var client = CreateClient(useParallelSearch, throwing, notFound);

            // Act
            var result = await client.SearchLyricAsync(Request, TestContext.Current.CancellationToken);

            // Assert: one provider answered, so the lyrics do not exist there.
            Assert.Equal(SearchResultCode.NotFound, result.Code);
            Assert.Equal(ResponseStatusCode.NoDataFound, result.ResponseStatusCode);
            Assert.Equal(Constants.ResponseMessages.NotFoundLyric, result.ResponseMessage);
            Assert.Equal(new[]
            {
                (ExternalProviderType.AZLyrics, ProviderAttemptOutcome.Error, "InvalidOperationException: boom"),
                (ExternalProviderType.Genius, ProviderAttemptOutcome.NotFound, string.Empty),
            }, Describe(result).OrderBy(attempt => attempt.ProviderType));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SearchLyricAsync_AllProvidersFailed_ReturnsAllProvidersFailed(bool useParallelSearch)
        {
            // Arrange: a real provider over HTTP 503 and a provider that throws.
            var (httpClient, _) = CreateHttpClient(new List<SentRequest>(), HttpStatusCode.ServiceUnavailable, "<html>busy</html>");
            var unavailable = new SongLyricsProvider();
            unavailable.WithHttpClient(httpClient);
            var throwing = CreateProvider(ExternalProviderType.AZLyrics, priority: 1, _ => throw new InvalidOperationException("boom"));
            var client = CreateClient(useParallelSearch, unavailable, throwing);

            // Act
            var result = await client.SearchLyricAsync(Request, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsEmpty());
            Assert.Equal(SearchResultCode.AllProvidersFailed, result.Code);
            Assert.Equal(ResponseStatusCode.Error, result.ResponseStatusCode);
            Assert.Equal(Constants.ResponseMessages.AllProvidersFailed, result.ResponseMessage);
            Assert.Equal(new[]
            {
                (ExternalProviderType.AZLyrics, ProviderAttemptOutcome.Error, "InvalidOperationException: boom"),
                (ExternalProviderType.SongLyrics, ProviderAttemptOutcome.Failed, "HTTP 503"),
            }, Describe(result).OrderBy(attempt => attempt.ProviderType));
        }

        [Fact]
        public async Task SearchLyricAsync_SiteAnswers404_IsNotFoundNotFailed()
        {
            // Arrange
            var (httpClient, _) = CreateHttpClient(new List<SentRequest>(), HttpStatusCode.NotFound, "<html>not found</html>");
            var provider = new SongLyricsProvider();
            provider.WithHttpClient(httpClient);
            var client = CreateClient(useParallelSearch: false, provider);

            // Act
            var result = await client.SearchLyricAsync(Request, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(SearchResultCode.NotFound, result.Code);
            Assert.Equal(new[] { (ExternalProviderType.SongLyrics, ProviderAttemptOutcome.NotFound, string.Empty) }, Describe(result));
        }

        [Fact]
        public async Task SearchLyricAsync_NetworkError_RecordsFailedWithoutStatusCode()
        {
            // Arrange
            var (httpClient, _) = CreateHttpClient(new List<SentRequest>(), (_, _) => throw new HttpRequestException("connection refused"));
            var provider = new AZLyricsProvider();
            provider.WithHttpClient(httpClient);
            var client = CreateClient(useParallelSearch: false, provider);

            // Act
            var result = await client.SearchLyricAsync(Request, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(SearchResultCode.AllProvidersFailed, result.Code);
            Assert.Equal(new[] { (ExternalProviderType.AZLyrics, ProviderAttemptOutcome.Failed, "No response (network error or timeout)") }, Describe(result));
        }

        [Fact]
        public async Task SearchLyricAsync_InstrumentalSong_ReturnsInstrumentalCodeAndAttempt()
        {
            // Arrange
            var provider = CreateProvider(ExternalProviderType.Lrclib, priority: 1,
                _ => Task.FromResult(new SearchResult(string.Empty, ExternalProviderType.Lrclib, SearchResultCode.Instrumental)));
            var client = CreateClient(useParallelSearch: false, provider);

            // Act
            var result = await client.SearchLyricAsync(Request, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(SearchResultCode.Instrumental, result.Code);
            Assert.True(result.Instrumental);
            Assert.Equal(new[] { (ExternalProviderType.Lrclib, ProviderAttemptOutcome.Instrumental, string.Empty) }, Describe(result));
        }

        [Fact]
        public async Task SearchLyricAsync_ParallelHigherPriorityFound_LowerRunningOneIsCanceled()
        {
            // Arrange: the lower priority provider waits until it is canceled. The attempt is recorded when the result is returned,
            // so the test does not wait for the provider task: it may start late on a busy thread pool (#87).
            // That the remaining providers are canceled is covered by the parallel search tests in LyricsScraperClientTests.
            var found = CreateProvider(ExternalProviderType.Genius, priority: 2, _ => Task.FromResult(new SearchResult("Lyrics", ExternalProviderType.Genius)));
            var hanging = CreateProvider(ExternalProviderType.AZLyrics, priority: 1, async token =>
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                return new SearchResult(ExternalProviderType.AZLyrics);
            });
            var client = CreateClient(useParallelSearch: true, found, hanging);

            // Act
            var result = await client.SearchLyricAsync(Request, TestContext.Current.CancellationToken).WaitAsync(SafetyTimeout, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(SearchResultCode.Found, result.Code);
            Assert.Equal(new[]
            {
                (ExternalProviderType.Genius, ProviderAttemptOutcome.Found, string.Empty),
                (ExternalProviderType.AZLyrics, ProviderAttemptOutcome.Canceled, Constants.ResponseMessages.ProviderStoppedResultFound),
            }, Describe(result));
        }

        [Theory]
        [MemberData(nameof(InvalidRequestsAndSetups))]
        public async Task SearchLyricAsync_InvalidRequestOrSetup_ReturnsCodeWithoutAttempts(
            Func<ILyricsScraperClient> createClient, SearchRequest searchRequest, string expectedCode, ResponseStatusCode expectedStatus, string expectedMessage)
        {
            // Arrange
            var client = createClient();

            // Act
            var result = await client.SearchLyricAsync(searchRequest, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(new SearchResultCode(expectedCode), result.Code);
            Assert.True(result.Code.IsInvalidRequestOrSetup);
            Assert.Equal(expectedStatus, result.ResponseStatusCode);
            Assert.Equal(expectedMessage, result.ResponseMessage);
            Assert.Empty(result.ProviderAttempts);
        }

        public static TheoryData<Func<ILyricsScraperClient>, SearchRequest, string, ResponseStatusCode, string> InvalidRequestsAndSetups()
        {
            static ILyricsScraperClient WithAZLyrics() => CreateClient(false,
                CreateProvider(ExternalProviderType.AZLyrics, 1, _ => Task.FromResult(new SearchResult(ExternalProviderType.AZLyrics))));

            return new()
            {
                { WithAZLyrics, null, SearchResultCode.Values.EmptyRequest, ResponseStatusCode.BadRequest, Constants.ResponseMessages.SearchRequestIsEmpty },
                { WithAZLyrics, new ArtistAndSongSearchRequest("", "Song"), SearchResultCode.Values.EmptyArtistOrSong, ResponseStatusCode.BadRequest, Constants.ResponseMessages.ArtistAndSongSearchRequestFieldsAreEmpty },
                { WithAZLyrics, new UriSearchRequest((Uri)null), SearchResultCode.Values.EmptyUri, ResponseStatusCode.BadRequest, Constants.ResponseMessages.UriSearchRequestFieldsAreEmpty },
                { WithAZLyrics, new ArtistAndSongSearchRequest("Artist", "Song", ExternalProviderType.Genius), SearchResultCode.Values.ProviderNotAvailable, ResponseStatusCode.BadRequest, Constants.ResponseMessages.ExternalProviderForRequestNotSpecified },
                { () => new LyricsScraperClient(), Request, SearchResultCode.Values.NoProviders, ResponseStatusCode.NoDataFound, Constants.ResponseMessages.ExternalProvidersListIsEmpty },
                { () => { var client = WithAZLyrics(); client.Disable(); return client; }, Request, SearchResultCode.Values.AllProvidersDisabled, ResponseStatusCode.NoDataFound, Constants.ResponseMessages.ExternalProvidersAreDisabled },
            };
        }

        private static readonly TimeSpan SafetyTimeout = TimeSpan.FromSeconds(10);

        private static IEnumerable<(ExternalProviderType ProviderType, ProviderAttemptOutcome Outcome, string Message)> Describe(SearchResult result)
            => result.ProviderAttempts.Select(attempt => (attempt.ProviderType, attempt.Outcome, attempt.Message));

        private static IExternalProvider CreateProvider(ExternalProviderType providerType, int priority, Func<CancellationToken, Task<SearchResult>> search)
        {
            var options = A.Fake<IExternalProviderOptions>();
            A.CallTo(() => options.ExternalProviderType).Returns(providerType);
            A.CallTo(() => options.Enabled).Returns(true);

            var enabled = true;
            var provider = A.Fake<IExternalProvider>();
            A.CallTo(() => provider.Options).Returns(options);
            A.CallTo(() => provider.IsEnabled).ReturnsLazily(() => enabled);
            A.CallTo(() => provider.Disable()).Invokes(() => enabled = false);
            A.CallTo(() => provider.SearchPriority).Returns(priority);
            A.CallTo(() => provider.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
                .ReturnsLazily((SearchRequest _, CancellationToken token) => search(token));
            return provider;
        }

        private static LyricsScraperClient CreateClient(bool useParallelSearch, params IExternalProvider[] providers)
        {
            var config = A.Fake<ILyricScraperClientConfig>();
            A.CallTo(() => config.UseParallelSearch).Returns(useParallelSearch);
            return new LyricsScraperClient(config, providers);
        }
    }
}
