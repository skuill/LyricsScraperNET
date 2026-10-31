using FakeItEasy;
using LyricsScraperNET.Common;
using LyricsScraperNET.Network;
using LyricsScraperNET.Providers.Lrclib;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static LyricsScraperNET.UnitTest.Network.HttpTestHelper;

namespace LyricsScraperNET.UnitTest.Network
{
    public class LrclibHttpClientTests
    {
        private static readonly Uri TestUri = new Uri("https://lrclib.example/api/get?artist_name=a&track_name=b");

        [Fact]
        public async Task LoadAsync_SuccessResponse_ReturnsBodyAndIdentifiesLibrary()
        {
            // Arrange
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, HttpStatusCode.OK, "{\"id\":1}");
            var webClient = new LrclibHttpClient(httpClient);

            // Act
            var result = await webClient.LoadAsync(TestUri, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal("{\"id\":1}", result);
            Assert.Equal(new[] { new SentRequest(HttpMethod.Get, TestUri, Constants.LibraryUserAgent, Constants.LibraryUserAgent) }, Snapshot(sentRequests));
            CallToSendAsync(handler).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LoadAsync_ClientWithOwnUserAgent_KeepsItAndStillSendsLibraryXUserAgent()
        {
            // Arrange
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, HttpStatusCode.OK, "{}");
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ConsumerApp/2.0");
            var webClient = new LrclibHttpClient(httpClient);

            // Act
            await webClient.LoadAsync(TestUri, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(new[] { new SentRequest(HttpMethod.Get, TestUri, "ConsumerApp/2.0", Constants.LibraryUserAgent) }, Snapshot(sentRequests));
            CallToSendAsync(handler).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LoadAsync_NotFound_ReturnsBodyForTheProvider()
        {
            // Arrange
            const string notFoundBody = "{\"code\":404,\"name\":\"TrackNotFound\"}";
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, HttpStatusCode.NotFound, notFoundBody);
            var webClient = new LrclibHttpClient(httpClient);

            // Act
            var result = await webClient.LoadAsync(TestUri, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(notFoundBody, result);
            CallToSendAsync(handler).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.Forbidden)]
        public async Task LoadAsync_UnsuccessfulStatusCode_ReturnsEmpty(HttpStatusCode statusCode)
        {
            // Arrange
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, statusCode, "<html>error page</html>");
            var webClient = new LrclibHttpClient(httpClient);

            // Act
            var result = await webClient.LoadAsync(TestUri, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(string.Empty, result);
            CallToSendAsync(handler).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LoadAsync_TooManyRequestsThenSuccess_RetriesOnceAndDisposesBothResponses()
        {
            // Arrange
            var rateLimitedContent = new TrackingContent("rate limited");
            var successContent = new TrackingContent("{\"id\":1}");
            var responses = new Queue<HttpResponseMessage>(new[]
            {
                CreateRateLimitedResponse(rateLimitedContent),
                new HttpResponseMessage(HttpStatusCode.OK) { Content = successContent },
            });
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, (_, _) => Task.FromResult(responses.Dequeue()));
            var webClient = new LrclibHttpClient(httpClient);

            // Act
            var result = await webClient.LoadAsync(TestUri, TestContext.Current.CancellationToken).WaitAsync(SafetyTimeout, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal("{\"id\":1}", result);
            Assert.True(rateLimitedContent.IsDisposed);
            Assert.True(successContent.IsDisposed);
            Assert.Equal(2, Snapshot(sentRequests).Count);
            CallToSendAsync(handler).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task LoadAsync_CallerCancelsDuringRetryDelay_ThrowsOperationCanceledException()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, (_, _) =>
            {
                // Retry-After is 30 s: the test passes only if the delay is canceled.
                cts.Cancel();
                return Task.FromResult(CreateRateLimitedResponse(new TrackingContent("rate limited"), TimeSpan.FromSeconds(30)));
            });
            var webClient = new LrclibHttpClient(httpClient);

            // Act & Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => webClient.LoadAsync(TestUri, cts.Token).WaitAsync(SafetyTimeout, TestContext.Current.CancellationToken));
            CallToSendAsync(handler).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LoadAsync_HttpClientTimeout_ReturnsEmptyInsteadOfThrowing()
        {
            // Arrange
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, async (_, ct) =>
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, ct);
                return CreateResponse(HttpStatusCode.OK, "too late");
            });
            httpClient.Timeout = TimeSpan.FromMilliseconds(50);
            var webClient = new LrclibHttpClient(httpClient);

            // Act
            var result = await webClient.LoadAsync(TestUri, TestContext.Current.CancellationToken).WaitAsync(SafetyTimeout, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(string.Empty, result);
            CallToSendAsync(handler).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LoadAsync_CanceledToken_ThrowsWithoutSendingRequest()
        {
            // Arrange
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, HttpStatusCode.OK, "{}");
            var webClient = new LrclibHttpClient(httpClient);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => webClient.LoadAsync(TestUri, cts.Token));
            CallToSendAsync(handler).MustNotHaveHappened();
        }

        [Fact]
        public void Constructor_Default_UsesSharedHttpClient()
        {
            // Act
            var webClient = new LrclibHttpClient();

            // Assert
            Assert.Same(SharedHttpClient.Instance, webClient.HttpClient);
        }

        private static HttpResponseMessage CreateRateLimitedResponse(HttpContent content, TimeSpan? retryAfter = null)
        {
            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = content };
            // The shortest delay the client accepts: Retry-After 0 is replaced with 1 s.
            response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(retryAfter ?? TimeSpan.FromSeconds(1));
            return response;
        }
    }
}
