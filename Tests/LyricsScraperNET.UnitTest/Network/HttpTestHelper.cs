using FakeItEasy;
using FakeItEasy.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.UnitTest.Network
{
    /// <summary>
    /// Network-free HTTP tests: the <see cref="HttpMessageHandler"/> under a real <see cref="HttpClient"/> is faked,
    /// so the client code (headers, status codes, disposal, cancellation) runs as in production.
    /// </summary>
    internal static class HttpTestHelper
    {
        /// <summary>
        /// The fake handler must outlive the test: it is never called after this timeout, so a broken implementation fails instead of hanging.
        /// </summary>
        internal static readonly TimeSpan SafetyTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// <see cref="HttpMessageHandler.SendAsync"/> is protected, so the call is matched by name.
        /// </summary>
        internal static IAnyCallConfigurationWithReturnTypeSpecified<Task<HttpResponseMessage>> CallToSendAsync(HttpMessageHandler handler)
            => A.CallTo(handler)
                .Where(call => call.Method.Name == "SendAsync")
                .WithReturnType<Task<HttpResponseMessage>>();

        /// <summary>
        /// Every request is copied to <paramref name="sentRequests"/>: the client disposes the request after sending it.
        /// </summary>
        internal static (HttpClient HttpClient, HttpMessageHandler Handler) CreateHttpClient(
            List<SentRequest> sentRequests,
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
        {
            var handler = A.Fake<HttpMessageHandler>();
            CallToSendAsync(handler).ReturnsLazily(call =>
            {
                var request = call.GetArgument<HttpRequestMessage>(0);
                lock (sentRequests)
                    sentRequests.Add(SentRequest.From(request));
                return respond(request, call.GetArgument<CancellationToken>(1));
            });

            return (new HttpClient(handler), handler);
        }

        internal static (HttpClient HttpClient, HttpMessageHandler Handler) CreateHttpClient(
            List<SentRequest> sentRequests, HttpStatusCode statusCode, string body)
            => CreateHttpClient(sentRequests, (_, _) => Task.FromResult(CreateResponse(statusCode, body)));

        internal static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string body)
            => new HttpResponseMessage(statusCode) { Content = new StringContent(body) };

        internal sealed record SentRequest(HttpMethod Method, Uri Uri, string UserAgent, string XUserAgent, string Authorization = null)
        {
            internal static SentRequest From(HttpRequestMessage request)
                => new SentRequest(request.Method, request.RequestUri, GetHeader(request, "User-Agent"), GetHeader(request, "X-User-Agent"), GetHeader(request, "Authorization"));

            private static string GetHeader(HttpRequestMessage request, string name)
                => request.Headers.NonValidated.TryGetValues(name, out var values) ? string.Join(" ", values) : null;
        }

        /// <summary>
        /// Response content that records whether the response was disposed.
        /// </summary>
        internal sealed class TrackingContent : HttpContent
        {
            private readonly byte[] _body;

            public TrackingContent(string body)
            {
                _body = System.Text.Encoding.UTF8.GetBytes(body);
            }

            public bool IsDisposed { get; private set; }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
                => stream.WriteAsync(_body, 0, _body.Length);

            protected override bool TryComputeLength(out long length)
            {
                length = _body.Length;
                return true;
            }

            protected override void Dispose(bool disposing)
            {
                IsDisposed = true;
                base.Dispose(disposing);
            }
        }

        internal static List<SentRequest> Snapshot(List<SentRequest> sentRequests)
        {
            lock (sentRequests)
                return sentRequests.ToList();
        }
    }
}
