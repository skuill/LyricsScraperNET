using FakeItEasy;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Genius;
using LyricsScraperNET.Providers.KPopLyrics;
using LyricsScraperNET.Providers.Lrclib;
using LyricsScraperNET.Providers.LyricsFreak;
using LyricsScraperNET.Providers.SongLyrics;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static LyricsScraperNET.UnitTest.Network.HttpTestHelper;

namespace LyricsScraperNET.UnitTest
{
    /// <summary>
    /// The sync API is obsolete (removed in 4.0.0, #79) and must not deadlock while it exists (#59).
    /// </summary>
    public class SyncApiTests
    {
        private const string DiagnosticId = "LSNET001";
        private const string IssueUrl = "https://github.com/skuill/LyricsScraperNET/issues/79";

        [Fact]
        public void SyncMembers_AllMarkedObsoleteWithDiagnosticIdAndIssueLink()
        {
            // Arrange
            var expected = new[]
            {
                "ILyricsScraperClient.SearchLyric(SearchRequest, CancellationToken)",
                "LyricsScraperClient.SearchLyric(SearchRequest, CancellationToken)",
                "IExternalProvider.SearchLyric(SearchRequest, CancellationToken)",
                "ExternalProviderBase.SearchLyric(SearchRequest, CancellationToken)",
                "ExternalProviderBase.SearchLyric(Uri, CancellationToken)",
                "ExternalProviderBase.SearchLyric(String, String, CancellationToken)",
                "IWebClient.Load(Uri, CancellationToken)",
                "NetHttpClient.Load(Uri, CancellationToken)",
                "LrclibHttpClient.Load(Uri, CancellationToken)",
            };

            // Act: every non-async SearchLyric / Load method declared in the library.
            var syncMembers = typeof(ILyricsScraperClient).Assembly.GetTypes()
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .Where(method => (method.Name == "SearchLyric" || method.Name == "Load") && !typeof(Task).IsAssignableFrom(method.ReturnType))
                .ToList();

            // Assert
            Assert.Equal(expected.OrderBy(x => x), syncMembers.Select(Describe).OrderBy(x => x));
            Assert.All(syncMembers, method =>
            {
                var obsolete = method.GetCustomAttribute<ObsoleteAttribute>();
                Assert.NotNull(obsolete);
                Assert.Equal(DiagnosticId, obsolete.DiagnosticId);
                Assert.Equal(IssueUrl, obsolete.UrlFormat);
                Assert.Contains("4.0.0", obsolete.Message);
                Assert.False(obsolete.IsError);
            });
        }

        [Theory]
        [MemberData(nameof(Providers))]
        public void ProviderSearchLyric_OnThreadWithBlockedSynchronizationContext_DoesNotDeadlock(Func<ExternalProviderBase> createProvider)
        {
            // Arrange
            var provider = createProvider();
            provider.WithWebClient(CreateDelayedWebClient());
            var context = new BlockedSynchronizationContext();

            // Act
            var result = RunOnThreadWithContext(context, () => provider.SearchLyric(new ArtistAndSongSearchRequest("Artist", "Song")));

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, context.PostCount);
        }

        [Fact]
        public void ClientSearchLyric_SequentialOnThreadWithBlockedSynchronizationContext_DoesNotDeadlock()
        {
            // Arrange: the sequential sync search calls the provider on the caller thread.
            var provider = new SongLyricsProvider();
            provider.WithWebClient(CreateDelayedWebClient());
            var client = new LyricsScraperClient { UseParallelSearch = false };
            client.AddProvider(provider);
            var context = new BlockedSynchronizationContext();

            // Act
            var result = RunOnThreadWithContext(context, () => client.SearchLyric(new ArtistAndSongSearchRequest("Artist", "Song")));

            // Assert
            Assert.Equal(ResponseStatusCode.NoDataFound, result.ResponseStatusCode);
            Assert.Equal(0, context.PostCount);
        }

        [Theory]
        [MemberData(nameof(Providers))]
        public async Task ProviderSearchLyricAsync_StartedWithSynchronizationContext_DoesNotPostToIt(Func<ExternalProviderBase> createProvider)
        {
            // Arrange
            var provider = createProvider();
            provider.WithWebClient(CreateDelayedWebClient());
            var context = new CountingSynchronizationContext();

            // Act
            var searchTask = StartWithContext(context, () => provider.SearchLyricAsync(new ArtistAndSongSearchRequest("Artist", "Song")));
            var result = await searchTask.WaitAsync(SafetyTimeout, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, context.PostCount);
        }

        public static TheoryData<Func<ExternalProviderBase>> Providers() => new()
        {
            () => new AZLyricsProvider(),
            () => new GeniusProvider(),
            () => new KPopLyricsProvider(),
            () => new LrclibProvider(),
            () => new LyricsFreakProvider(),
            () => new SongLyricsProvider(),
        };

        /// <summary>
        /// Every load completes later on a timer thread, so the provider awaits an incomplete task.
        /// </summary>
        private static IWebClient CreateDelayedWebClient()
        {
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).ReturnsLazily(() => LoadLaterAsync());
            return webClient;
        }

        private static async Task<string> LoadLaterAsync()
        {
            await Task.Delay(20).ConfigureAwait(false);
            return string.Empty;
        }

        private static T RunOnThreadWithContext<T>(SynchronizationContext context, Func<T> action)
        {
            T result = default;
            Exception error = null;
            var thread = new Thread(() =>
            {
                SynchronizationContext.SetSynchronizationContext(context);
                try
                {
                    result = action();
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            })
            {
                // A deadlocked thread must not keep the test process alive.
                IsBackground = true,
            };

            thread.Start();
            Assert.True(thread.Join(SafetyTimeout), "The sync call did not complete: deadlock on the caller's SynchronizationContext.");
            Assert.Null(error);
            return result;
        }

        private static Task<T> StartWithContext<T>(SynchronizationContext context, Func<Task<T>> action)
        {
            var previous = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                return action();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        private static string Describe(MethodInfo method)
            => $"{method.DeclaringType!.Name}.{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})";

        /// <summary>
        /// Like a UI thread blocked in a sync call: posted continuations never run.
        /// </summary>
        private sealed class BlockedSynchronizationContext : SynchronizationContext
        {
            private int _postCount;

            public int PostCount => Volatile.Read(ref _postCount);

            public override void Post(SendOrPostCallback d, object state) => Interlocked.Increment(ref _postCount);

            public override void Send(SendOrPostCallback d, object state) => Interlocked.Increment(ref _postCount);
        }

        /// <summary>
        /// Runs posted continuations on the thread pool and counts them.
        /// </summary>
        private sealed class CountingSynchronizationContext : SynchronizationContext
        {
            private int _postCount;

            public int PostCount => Volatile.Read(ref _postCount);

            public override void Post(SendOrPostCallback d, object state)
            {
                Interlocked.Increment(ref _postCount);
                base.Post(d, state);
            }
        }
    }
}
