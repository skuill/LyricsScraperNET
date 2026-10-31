using FakeItEasy;
using LyricsScraperNET.Network;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Network.Models;
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
    /// <summary>
    /// <see cref="IWebClientWithStatus.LoadWithStatusAsync"/> of the library web clients (#74).
    /// </summary>
    public class WebLoadStatusTests
    {
        private static readonly Uri TestUri = new("https://lyrics.example/song");

        [Theory]
        [InlineData(HttpStatusCode.OK, "<html>lyric</html>", "Success", "<html>lyric</html>")]
        [InlineData(HttpStatusCode.OK, "", "Success", "")]
        [InlineData(HttpStatusCode.NotFound, "<html>not found</html>", "NotFound", "")]
        [InlineData(HttpStatusCode.Forbidden, "<html>blocked</html>", "Failed", "")]
        [InlineData(HttpStatusCode.TooManyRequests, "slow down", "Failed", "")]
        [InlineData(HttpStatusCode.ServiceUnavailable, "busy", "Failed", "")]
        public async Task NetHttpClient_StatusCode_MapsToLoadStatus(HttpStatusCode statusCode, string body, string expectedStatus, string expectedBody)
        {
            // Arrange
            var (httpClient, _) = CreateHttpClient(new List<SentRequest>(), statusCode, body);
            var webClient = new NetHttpClient(httpClient);

            // Act
            var result = await webClient.LoadWithStatusAsync(TestUri, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(new WebLoadResult(expectedBody, Enum.Parse<WebLoadStatus>(expectedStatus), (int)statusCode), result);
            Assert.Equal(expectedBody, await webClient.LoadAsync(TestUri, TestContext.Current.CancellationToken));
        }

        [Theory]
        [MemberData(nameof(NoResponseCases))]
        public async Task NetHttpClient_NoResponse_FailedWithoutStatusCode(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond, bool shortTimeout)
        {
            // Arrange
            var (httpClient, _) = CreateHttpClient(new List<SentRequest>(), respond);
            if (shortTimeout)
                httpClient.Timeout = TimeSpan.FromMilliseconds(50);
            var webClient = new NetHttpClient(httpClient);

            // Act
            var result = await webClient.LoadWithStatusAsync(TestUri, TestContext.Current.CancellationToken).WaitAsync(SafetyTimeout, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(new WebLoadResult(string.Empty, WebLoadStatus.Failed, null), result);
            Assert.Equal("No response (network error or timeout)", result.FailureMessage);
        }

        public static TheoryData<Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>, bool> NoResponseCases() => new()
        {
            { (_, _) => throw new HttpRequestException("connection refused"), false },
            { (_, _) => throw new InvalidOperationException("unexpected"), false },
            { async (_, ct) => { await Task.Delay(Timeout.InfiniteTimeSpan, ct); return CreateResponse(HttpStatusCode.OK, "too late"); }, true },
        };

        [Theory]
        [InlineData(HttpStatusCode.OK, "{\"id\":1}", "Success", "{\"id\":1}")]
        [InlineData(HttpStatusCode.NotFound, "{\"code\":404,\"name\":\"TrackNotFound\"}", "NotFound", "{\"code\":404,\"name\":\"TrackNotFound\"}")]
        [InlineData(HttpStatusCode.ServiceUnavailable, "{\"name\":\"ServerOverloaded\"}", "Failed", "")]
        [InlineData(HttpStatusCode.InternalServerError, "error", "Failed", "")]
        public async Task LrclibHttpClient_StatusCode_MapsToLoadStatusAndKeeps404Body(HttpStatusCode statusCode, string body, string expectedStatus, string expectedBody)
        {
            // Arrange
            var (httpClient, _) = CreateHttpClient(new List<SentRequest>(), statusCode, body);
            var webClient = new LrclibHttpClient(httpClient);

            // Act
            var result = await webClient.LoadWithStatusAsync(TestUri, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(new WebLoadResult(expectedBody, Enum.Parse<WebLoadStatus>(expectedStatus), (int)statusCode), result);
        }

        [Fact]
        public async Task LrclibHttpClient_RateLimitedTwice_FailedWith429()
        {
            // Arrange
            var (httpClient, handler) = CreateHttpClient(new List<SentRequest>(), (_, _) =>
            {
                var response = CreateResponse(HttpStatusCode.TooManyRequests, "rate limited");
                response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromMilliseconds(10));
                return Task.FromResult(response);
            });
            var webClient = new LrclibHttpClient(httpClient);

            // Act
            var result = await webClient.LoadWithStatusAsync(TestUri, TestContext.Current.CancellationToken).WaitAsync(SafetyTimeout, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(new WebLoadResult(string.Empty, WebLoadStatus.Failed, 429), result);
            Assert.Equal("HTTP 429", result.FailureMessage);
            CallToSendAsync(handler).MustHaveHappenedTwiceExactly();
        }
    }
}
