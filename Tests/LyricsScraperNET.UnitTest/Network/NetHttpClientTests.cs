using FakeItEasy;
using LyricsScraperNET.Common;
using LyricsScraperNET.Network;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static LyricsScraperNET.UnitTest.Network.HttpTestHelper;

namespace LyricsScraperNET.UnitTest.Network
{
    public class NetHttpClientTests
    {
        private static readonly Uri TestUri = new Uri("https://lyrics.example/song");

        [Fact]
        public async Task LoadAsync_SuccessResponse_ReturnsBodyAndSendsOneGetWithLibraryUserAgent()
        {
            // Arrange
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, HttpStatusCode.OK, "<html>lyric</html>");
            var webClient = new NetHttpClient(httpClient);

            // Act
            var result = await webClient.LoadAsync(TestUri, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal("<html>lyric</html>", result);
            Assert.Equal(new[] { new SentRequest(HttpMethod.Get, TestUri, Constants.LibraryUserAgent, null) }, Snapshot(sentRequests));
            CallToSendAsync(handler).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LoadAsync_ClientWithOwnUserAgent_SendsConsumerUserAgent()
        {
            // Arrange
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, HttpStatusCode.OK, "body");
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ConsumerApp/2.0");
            var webClient = new NetHttpClient(httpClient);

            // Act
            var result = await webClient.LoadAsync(TestUri, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal("body", result);
            Assert.Equal(new[] { new SentRequest(HttpMethod.Get, TestUri, "ConsumerApp/2.0", null) }, Snapshot(sentRequests));
            CallToSendAsync(handler).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        public async Task LoadAsync_UnsuccessfulStatusCode_ReturnsEmptyInsteadOfErrorPage(HttpStatusCode statusCode)
        {
            // Arrange
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, statusCode, "<html>error page</html>");
            var webClient = new NetHttpClient(httpClient);

            // Act
            var result = await webClient.LoadAsync(TestUri, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(string.Empty, result);
            CallToSendAsync(handler).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LoadAsync_SuccessWithEmptyBody_ReturnsEmptyWithoutThrowing()
        {
            // Arrange
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, HttpStatusCode.OK, string.Empty);
            var webClient = new NetHttpClient(httpClient);

            // Act
            var result = await webClient.LoadAsync(TestUri, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(string.Empty, result);
            CallToSendAsync(handler).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [MemberData(nameof(NetworkExceptions))]
        public async Task LoadAsync_NetworkOrUnexpectedError_ReturnsEmpty(Exception exception)
        {
            // Arrange
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, (_, _) => Task.FromException<HttpResponseMessage>(exception));
            var webClient = new NetHttpClient(httpClient);

            // Act
            var result = await webClient.LoadAsync(TestUri, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(string.Empty, result);
            CallToSendAsync(handler).MustHaveHappenedOnceExactly();
        }

        public static TheoryData<Exception> NetworkExceptions() => new()
        {
            new HttpRequestException("Name or service not known"),
            new InvalidOperationException("Unexpected"),
        };

        [Fact]
        public async Task LoadAsync_CallerCancelsDuringRequest_ThrowsOperationCanceledException()
        {
            // Arrange
            var requestStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, async (_, ct) =>
            {
                requestStarted.SetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, ct);
                return CreateResponse(HttpStatusCode.OK, "too late");
            });
            var webClient = new NetHttpClient(httpClient);
            using var cts = new CancellationTokenSource();

            // Act
            var loadTask = webClient.LoadAsync(TestUri, cts.Token);
            await requestStarted.Task.WaitAsync(SafetyTimeout, TestContext.Current.CancellationToken);
            cts.Cancel();

            // Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => loadTask.WaitAsync(SafetyTimeout, TestContext.Current.CancellationToken));
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
            var webClient = new NetHttpClient(httpClient);

            // Act
            var result = await webClient.LoadAsync(TestUri, TestContext.Current.CancellationToken).WaitAsync(SafetyTimeout, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(string.Empty, result);
            CallToSendAsync(handler).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.NotFound)]
        public async Task LoadAsync_AnyResponse_IsDisposed(HttpStatusCode statusCode)
        {
            // Arrange
            var content = new TrackingContent("body");
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests,
                (_, _) => Task.FromResult(new HttpResponseMessage(statusCode) { Content = content }));
            var webClient = new NetHttpClient(httpClient);

            // Act
            await webClient.LoadAsync(TestUri, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(content.IsDisposed);
            CallToSendAsync(handler).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void Load_SuccessResponse_ReturnsBody()
        {
            // Arrange
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, HttpStatusCode.OK, "sync body");
            var webClient = new NetHttpClient(httpClient);

            // Act
            var result = webClient.Load(TestUri, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal("sync body", result);
            Assert.Equal(new[] { new SentRequest(HttpMethod.Get, TestUri, Constants.LibraryUserAgent, null) }, Snapshot(sentRequests));
            CallToSendAsync(handler).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void Load_CanceledToken_ThrowsWithoutSendingRequest()
        {
            // Arrange
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, HttpStatusCode.OK, "body");
            var webClient = new NetHttpClient(httpClient);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            Assert.ThrowsAny<OperationCanceledException>(() => webClient.Load(TestUri, cts.Token));
            CallToSendAsync(handler).MustNotHaveHappened();
        }

        /// <summary>
        /// Expected failures (site errors, timeouts, network) are warnings; only unexpected exceptions are errors.
        /// </summary>
        [Theory]
        [InlineData("status", LogLevel.Warning, "Request to https://lyrics.example/song returned status code 503.")]
        [InlineData("timeout", LogLevel.Warning, "Request to https://lyrics.example/song timed out.")]
        [InlineData("network", LogLevel.Warning, "Request to https://lyrics.example/song failed.")]
        [InlineData("unexpected", LogLevel.Error, "Unexpected error while loading https://lyrics.example/song.")]
        public async Task LoadAsync_Failure_LogsWithExpectedLevel(string failure, LogLevel expectedLevel, string expectedMessage)
        {
            // Arrange
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, async (_, ct) =>
            {
                switch (failure)
                {
                    case "status":
                        return CreateResponse(HttpStatusCode.ServiceUnavailable, "down");
                    case "timeout":
                        await Task.Delay(Timeout.InfiniteTimeSpan, ct);
                        return CreateResponse(HttpStatusCode.OK, "too late");
                    case "network":
                        throw new HttpRequestException("Connection refused");
                    default:
                        throw new InvalidOperationException("Unexpected");
                }
            });
            httpClient.Timeout = TimeSpan.FromMilliseconds(50);
            // ILogger<NetHttpClient> of an internal type cannot be faked, so Logger<T> forwards to a fake ILogger.
            var logger = A.Fake<ILogger>();
            var loggerFactory = A.Fake<ILoggerFactory>();
            A.CallTo(() => loggerFactory.CreateLogger(A<string>._)).Returns(logger);
            A.CallTo(() => logger.IsEnabled(A<LogLevel>._)).Returns(true);
            var webClient = new NetHttpClient(httpClient, new Logger<NetHttpClient>(loggerFactory));

            // Act
            var result = await webClient.LoadAsync(TestUri, TestContext.Current.CancellationToken).WaitAsync(SafetyTimeout, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(string.Empty, result);
            var logCalls = Fake.GetCalls(logger).Where(call => call.Method.Name == nameof(ILogger.Log)).ToList();
            Assert.Equal(
                new[] { (expectedLevel, expectedMessage) },
                logCalls.Select(call => (call.GetArgument<LogLevel>(0), call.Arguments[2].ToString())));
            CallToSendAsync(handler).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void Constructor_Default_UsesSharedHttpClient()
        {
            // Act
            var first = new NetHttpClient();
            var second = new NetHttpClient();

            // Assert
            Assert.Same(SharedHttpClient.Instance, first.HttpClient);
            Assert.Same(SharedHttpClient.Instance, second.HttpClient);
        }

        [Fact]
        public void Constructor_NullHttpClient_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new NetHttpClient((HttpClient)null));
        }
    }
}
