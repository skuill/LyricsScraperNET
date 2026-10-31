using FakeItEasy;
using LyricsScraperNET.Common;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Network;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Providers.Genius;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.TestShared.Extensions;
using LyricsScraperNET.TestShared.Providers;
using LyricsScraperNET.TestShared.TestModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static LyricsScraperNET.UnitTest.Network.HttpTestHelper;

namespace LyricsScraperNET.UnitTest.Providers.Genius
{
    public class GeniusProviderTest : ProviderTestBase
    {
        private const string Artist = "Test Artist";
        private const string Song = "Test Song";
        private const string ApiKey = "test-api-key";
        private const string LyricPagePath = "Providers/Genius/Resources/Lyrics_HtmlPage_01.txt";
        private const string LyricResultPath = "Providers/Genius/Resources/Lyrics_Result_01.txt";

        private static readonly Uri LyricUri = new Uri("https://genius.com/Test-artist-test-song-lyrics");
        private static readonly Uri SearchUri = new GeniusUriConverter().GetLyricUri(Artist, Song);
        private static readonly Uri ApiSearchUri = new GeniusUriConverter().GetApiSearchUri(Artist, Song);

        #region Lyric page

        [Theory]
        [MemberData(nameof(GetTestData), arguments: "Providers\\Genius\\lyric_test_data.json")]
        public void SearchLyric_UnitDynamicData_Success(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new GeniusProvider();
            lyricsClient.ConfigureExternalProvider(testData);

            SearchRequest searchRequest = CreateSearchRequest(testData);
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            AssertSuccess(testData.LyricResultData, searchResult);
        }

        [Theory]
        [MemberData(nameof(GetTestData), arguments: "Providers\\Genius\\lyric_test_data.json")]
        public async Task SearchLyricAsync_UnitDynamicData_LoadsLyricUriAndReturnsSuccess(LyricsTestData testData)
        {
            // Arrange
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).Returns(testData.LyricPageData);

            var lyricsClient = new GeniusProvider();
            lyricsClient.WithWebClient(webClient);

            var searchRequest = CreateSearchRequest(testData);
            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(searchRequest, cancellationToken);

            // Assert
            AssertSuccess(testData.LyricResultData, searchResult);
            A.CallTo(() => webClient.LoadAsync(new Uri(testData.SongUri), cancellationToken)).MustHaveHappenedOnceExactly();
            A.CallTo(() => webClient.Load(A<Uri>._, A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Theory]
        [MemberData(nameof(GetTestData), arguments: "Providers\\Genius\\instrumental_test_data.json")]
        public void SearchLyric_UnitDynamicData_Instrumental(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new GeniusProvider();
            lyricsClient.ConfigureExternalProvider(testData);

            SearchRequest searchRequest = CreateSearchRequest(testData);
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var searchResult = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.Genius, searchResult.ExternalProviderType);
            Assert.True(searchResult.Instrumental);
        }

        /// <summary>
        /// A page without lyric containers, a page with empty containers and an empty body (failed request).
        /// </summary>
        [Theory]
        [MemberData(nameof(GetTestData), arguments: "Providers\\Genius\\no_lyrics_test_data.json")]
        public async Task SearchLyricAsync_PageWithoutLyrics_ReturnsNoDataFound(LyricsTestData testData)
        {
            // Arrange
            var lyricsClient = new GeniusProvider();
            lyricsClient.ConfigureExternalProvider(testData);

            var searchRequest = CreateSearchRequest(testData);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(searchRequest, TestContext.Current.CancellationToken);

            // Assert
            AssertNoDataFound(searchResult);
        }

        #endregion

        #region Search by artist and song

        [Theory]
        [InlineData("Test Artist", "Test Song", "Test Artist", "Test Song")]
        [InlineData("test artist", "TEST SONG", "Test Artist", "Test Song")]
        [InlineData(" Test   Artist ", "Test Song", "Test\u00A0Artist", "Test\u200B Song")]
        [InlineData("Test Artist", "Don't Stop", "Test Artist", "Don\u2019t Stop")]
        [InlineData("Test Artist", "Dont Stop", "Test Artist", "Don\u2019t Stop")]
        [InlineData("Test Artist", "\u2018Til Dawn", "Test Artist", "'Til Dawn")]
        [InlineData("Test Artist", "Rock & Roll", "Test Artist", "Rock and Roll")]
        [InlineData("Test & Artist", "Song", "Test and Artist", "Song")]
        [InlineData("Beyonce", "Halo", "Beyoncé", "Halo")]
        [InlineData("Jay-Z", "Empire State of Mind", "JAŸ-Z", "Empire State of Mind")]
        [InlineData("Ze Ramalho", "Avohai", "Zé Ramalho", "Avôhai")]
        [InlineData("Attack Attack", "Stick Stickly", "Attack Attack!", "Stick Stickly")]
        [InlineData("Attack Attack!", "What Happens If I Cant Check My MySpace When We Get There", "Attack Attack!", "What Happens If I Can\u2019t Check My MySpace When We Get There?")]
        [InlineData("Daft Punk", "Harder, Better, Faster, Stronger", "Daft Punk", "Harder Better Faster Stronger")]
        [InlineData("Attack Attack!", "Dr. Shavargo Part 3", "Attack Attack!", "Dr. Shavargo, Pt. 3")]
        [InlineData("!!!", "Song", "!!!", "Song")]
        [InlineData("Кино", "Группа крови", "КИНО", "Группа Крови")]
        public async Task SearchLyricAsync_HitWithSameArtistAndTitle_LoadsItsPage(string artist, string song, string hitArtist, string hitTitle)
        {
            // Arrange
            var searchUri = new GeniusUriConverter().GetLyricUri(artist, song);
            var searchResponse = CreateSearchResponse(
                ("Genius Translations", $"{hitArtist} - {hitTitle} (Translation)", "https://genius.com/translation-lyrics"),
                (hitArtist, $"{hitTitle} (Live)", "https://genius.com/live-lyrics"),
                (hitArtist, hitTitle, LyricUri.AbsoluteUri));
            var cancellationToken = TestContext.Current.CancellationToken;

            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(searchUri, cancellationToken)).Returns(searchResponse);
            A.CallTo(() => webClient.LoadAsync(LyricUri, cancellationToken)).Returns(File.ReadAllText(LyricPagePath));

            var lyricsClient = new GeniusProvider();
            lyricsClient.WithWebClient(webClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest(artist, song), cancellationToken);

            // Assert
            AssertSuccess(File.ReadAllText(LyricResultPath), searchResult);
            A.CallTo(() => webClient.LoadAsync(searchUri, cancellationToken)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => webClient.LoadAsync(LyricUri, cancellationToken)).MustHaveHappenedOnceExactly());
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustHaveHappenedTwiceExactly();
            A.CallTo(() => webClient.Load(A<Uri>._, A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Theory]
        [MemberData(nameof(NoMatchingHitResponses))]
        public async Task SearchLyricAsync_NoHitWithSameArtistAndTitle_ReturnsNoDataFoundWithoutLoadingPage(string searchResponse)
        {
            // Arrange
            var webClient = CreateWebClient(searchResponse);
            var lyricsClient = new GeniusProvider();
            lyricsClient.WithWebClient(webClient);
            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest(Artist, Song), cancellationToken);

            // Assert
            AssertNoDataFound(searchResult);
            A.CallTo(() => webClient.LoadAsync(SearchUri, cancellationToken)).MustHaveHappenedOnceExactly();
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        public static TheoryData<string> NoMatchingHitResponses() => new()
        {
            // Same artist, another song: the first hit used to be taken without checks.
            CreateSearchResponse((Artist, "Another Song", "https://genius.com/another-song-lyrics")),
            // Another artist, same title.
            CreateSearchResponse(("Another Artist", Song, "https://genius.com/another-artist-lyrics")),
            // Live version or remix of the song.
            CreateSearchResponse((Artist, $"{Song} (Live)", "https://genius.com/live-lyrics")),
            // Translation page of the song.
            CreateSearchResponse(("Genius Translations", $"{Artist} - {Song} (Translation)", "https://genius.com/translation-lyrics")),
            // Text without letters and digits is compared as is.
            CreateSearchResponse(("!!!", Song, "https://genius.com/other-lyrics")),
            CreateSearchResponse(),
            // Matching hit with an invalid url.
            CreateSearchResponse((Artist, Song, "not a url")),
            CreateSearchResponse((Artist, Song, null)),
        };

        [Theory]
        [MemberData(nameof(InvalidSearchResponses))]
        public async Task SearchLyricAsync_InvalidSearchResponse_ReturnsNoDataFound(string searchResponse)
        {
            // Arrange
            var webClient = CreateWebClient(searchResponse);
            var lyricsClient = new GeniusProvider();
            lyricsClient.WithWebClient(webClient);
            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest(Artist, Song), cancellationToken);

            // Assert
            AssertNoDataFound(searchResult);
            A.CallTo(() => webClient.LoadAsync(SearchUri, cancellationToken)).MustHaveHappenedOnceExactly();
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        public static TheoryData<string> InvalidSearchResponses() => new()
        {
            string.Empty,
            "<html>error page</html>",
            "[]",
            "{}",
            "{\"response\":[]}",
            "{\"response\":{}}",
            "{\"response\":{\"hits\":{}}}",
        };

        [Fact]
        public async Task SearchLyricAsync_MalformedHitsBeforeMatchingHit_SkipsThem()
        {
            // Arrange
            var malformedHits = new object[]
            {
                "hit",
                new { result = "result" },
                new { result = new { title = Song, url = "https://genius.com/no-artist-lyrics" } },
                new { result = new { title = Song, url = "https://genius.com/artist-not-object-lyrics", primary_artist = "artist" } },
                new { result = new { title = 1, url = "https://genius.com/title-not-string-lyrics", primary_artist = new { name = Artist } } },
                new { result = new { title = Song, url = "https://genius.com/name-not-string-lyrics", primary_artist = new { name = 1 } } },
                CreateHit(Artist, Song, LyricUri.AbsoluteUri),
            };
            var searchResponse = JsonSerializer.Serialize(new { response = new { hits = malformedHits } });
            var webClient = CreateWebClient(searchResponse, File.ReadAllText(LyricPagePath));
            var lyricsClient = new GeniusProvider();
            lyricsClient.WithWebClient(webClient);
            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest(Artist, Song), cancellationToken);

            // Assert
            AssertSuccess(File.ReadAllText(LyricResultPath), searchResult);
            A.CallTo(() => webClient.LoadAsync(SearchUri, cancellationToken)).MustHaveHappenedOnceExactly();
            A.CallTo(() => webClient.LoadAsync(LyricUri, cancellationToken)).MustHaveHappenedOnceExactly();
        }

        #endregion

        #region API key

        [Fact]
        public async Task SearchLyricAsync_WithApiKey_UsesApiSearchOnly()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(ApiSearchUri, cancellationToken)).Returns(CreateSearchResponse((Artist, Song, LyricUri.AbsoluteUri)));
            A.CallTo(() => webClient.LoadAsync(LyricUri, cancellationToken)).Returns(File.ReadAllText(LyricPagePath));

            var lyricsClient = new GeniusProvider(new GeniusOptions { Enabled = true, ApiKey = ApiKey });
            lyricsClient.WithWebClient(webClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest(Artist, Song), cancellationToken);

            // Assert
            AssertSuccess(File.ReadAllText(LyricResultPath), searchResult);
            A.CallTo(() => webClient.LoadAsync(ApiSearchUri, cancellationToken)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => webClient.LoadAsync(LyricUri, cancellationToken)).MustHaveHappenedOnceExactly());
            A.CallTo(() => webClient.LoadAsync(SearchUri, A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task SearchLyricAsync_WithApiKeyAndNoMatchingHit_DoesNotFallBackToPublicSearch()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(ApiSearchUri, cancellationToken)).Returns(CreateSearchResponse((Artist, "Another Song", LyricUri.AbsoluteUri)));
            A.CallTo(() => webClient.LoadAsync(SearchUri, cancellationToken)).Returns(CreateSearchResponse((Artist, Song, LyricUri.AbsoluteUri)));

            var lyricsClient = new GeniusProvider(new GeniusOptions { Enabled = true, ApiKey = ApiKey });
            lyricsClient.WithWebClient(webClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest(Artist, Song), cancellationToken);

            // Assert
            AssertNoDataFound(searchResult);
            A.CallTo(() => webClient.LoadAsync(ApiSearchUri, cancellationToken)).MustHaveHappenedOnceExactly();
            A.CallTo(() => webClient.LoadAsync(A<Uri>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData("")]
        [InlineData("<html>error page</html>")]
        public async Task SearchLyricAsync_WithApiKeyAndFailedApiSearch_FallsBackToPublicSearch(string apiSearchResponse)
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(ApiSearchUri, cancellationToken)).Returns(apiSearchResponse);
            A.CallTo(() => webClient.LoadAsync(SearchUri, cancellationToken)).Returns(CreateSearchResponse((Artist, Song, LyricUri.AbsoluteUri)));
            A.CallTo(() => webClient.LoadAsync(LyricUri, cancellationToken)).Returns(File.ReadAllText(LyricPagePath));

            var lyricsClient = new GeniusProvider(new GeniusOptions { Enabled = true, ApiKey = ApiKey });
            lyricsClient.WithWebClient(webClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest(Artist, Song), cancellationToken);

            // Assert
            AssertSuccess(File.ReadAllText(LyricResultPath), searchResult);
            A.CallTo(() => webClient.LoadAsync(ApiSearchUri, cancellationToken)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => webClient.LoadAsync(SearchUri, cancellationToken)).MustHaveHappenedOnceExactly())
                .Then(A.CallTo(() => webClient.LoadAsync(LyricUri, cancellationToken)).MustHaveHappenedOnceExactly());
        }

        [Fact]
        public async Task SearchLyricAsync_WithApiKey_SendsKeyOnlyToApi()
        {
            // Arrange
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, (request, _) => Task.FromResult(
                request.RequestUri == ApiSearchUri
                    ? CreateResponse(HttpStatusCode.OK, CreateSearchResponse((Artist, Song, LyricUri.AbsoluteUri)))
                    : CreateResponse(HttpStatusCode.OK, File.ReadAllText(LyricPagePath))));

            var lyricsClient = new GeniusProvider(new GeniusOptions { Enabled = true, ApiKey = ApiKey });
            lyricsClient.WithHttpClient(httpClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest(Artist, Song), TestContext.Current.CancellationToken);

            // Assert
            AssertSuccess(File.ReadAllText(LyricResultPath), searchResult);
            Assert.Equal(new[]
            {
                new SentRequest(HttpMethod.Get, ApiSearchUri, Constants.LibraryUserAgent, null, $"Bearer {ApiKey}"),
                new SentRequest(HttpMethod.Get, LyricUri, Constants.LibraryUserAgent, null),
            }, Snapshot(sentRequests));
            CallToSendAsync(handler).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task SearchLyricAsync_WithRejectedApiKey_FallsBackToPublicSearchWithoutKey()
        {
            // Arrange
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, (request, _) => Task.FromResult(
                request.RequestUri == ApiSearchUri
                    ? CreateResponse(HttpStatusCode.Unauthorized, "{\"error\":\"invalid_token\"}")
                    : request.RequestUri == SearchUri
                        ? CreateResponse(HttpStatusCode.OK, CreateSearchResponse((Artist, Song, LyricUri.AbsoluteUri)))
                        : CreateResponse(HttpStatusCode.OK, File.ReadAllText(LyricPagePath))));

            var lyricsClient = new GeniusProvider(new GeniusOptions { Enabled = true, ApiKey = ApiKey });
            lyricsClient.WithHttpClient(httpClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest(Artist, Song), TestContext.Current.CancellationToken);

            // Assert
            AssertSuccess(File.ReadAllText(LyricResultPath), searchResult);
            Assert.Equal(new[]
            {
                new SentRequest(HttpMethod.Get, ApiSearchUri, Constants.LibraryUserAgent, null, $"Bearer {ApiKey}"),
                new SentRequest(HttpMethod.Get, SearchUri, Constants.LibraryUserAgent, null),
                new SentRequest(HttpMethod.Get, LyricUri, Constants.LibraryUserAgent, null),
            }, Snapshot(sentRequests));
            CallToSendAsync(handler).MustHaveHappenedANumberOfTimesMatching(times => times == 3);
        }

        [Fact]
        public async Task SearchLyricAsync_WithoutApiKey_UsesPublicSearchWithoutKey()
        {
            // Arrange
            var sentRequests = new List<SentRequest>();
            var (httpClient, handler) = CreateHttpClient(sentRequests, (request, _) => Task.FromResult(
                request.RequestUri == SearchUri
                    ? CreateResponse(HttpStatusCode.OK, CreateSearchResponse((Artist, Song, LyricUri.AbsoluteUri)))
                    : CreateResponse(HttpStatusCode.OK, File.ReadAllText(LyricPagePath))));

            var lyricsClient = new GeniusProvider();
            lyricsClient.WithHttpClient(httpClient);

            // Act
            var searchResult = await lyricsClient.SearchLyricAsync(new ArtistAndSongSearchRequest(Artist, Song), TestContext.Current.CancellationToken);

            // Assert
            AssertSuccess(File.ReadAllText(LyricResultPath), searchResult);
            Assert.Equal(new[]
            {
                new SentRequest(HttpMethod.Get, SearchUri, Constants.LibraryUserAgent, null),
                new SentRequest(HttpMethod.Get, LyricUri, Constants.LibraryUserAgent, null),
            }, Snapshot(sentRequests));
            CallToSendAsync(handler).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public void Constructor_Default_WebClientAddsApiKeyOnlyForApiRequests()
        {
            // Arrange
            var lyricsClient = new GeniusProvider();
            ((GeniusOptions)lyricsClient.Options).ApiKey = ApiKey;
            var configureRequest = Assert.IsType<NetHttpClient>(lyricsClient.WebClient).ConfigureRequest;
            Assert.NotNull(configureRequest);
            using var apiRequest = new HttpRequestMessage(HttpMethod.Get, ApiSearchUri);
            using var pageRequest = new HttpRequestMessage(HttpMethod.Get, LyricUri);

            // Act
            configureRequest(apiRequest);
            configureRequest(pageRequest);

            // Assert
            Assert.Equal($"Bearer {ApiKey}", apiRequest.Headers.Authorization?.ToString());
            Assert.Null(pageRequest.Headers.Authorization);
        }

        #endregion

        private static IWebClient CreateWebClient(string searchResponse, string lyricPage = "")
        {
            var webClient = A.Fake<IWebClient>();
            A.CallTo(() => webClient.LoadAsync(SearchUri, A<CancellationToken>._)).Returns(searchResponse);
            A.CallTo(() => webClient.LoadAsync(LyricUri, A<CancellationToken>._)).Returns(lyricPage);
            return webClient;
        }

        private static string CreateSearchResponse(params (string Artist, string Title, string Url)[] hits)
            => JsonSerializer.Serialize(new
            {
                meta = new { status = 200 },
                response = new { hits = hits.Select(hit => CreateHit(hit.Artist, hit.Title, hit.Url)) },
            });

        private static object CreateHit(string artist, string title, string url)
            => new { type = "song", result = new { title, url, primary_artist = new { name = artist } } };

        private static void AssertSuccess(string expectedLyric, SearchResult searchResult)
        {
            Assert.NotNull(searchResult);
            Assert.False(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.Genius, searchResult.ExternalProviderType);
            Assert.Equal(expectedLyric.Replace("\r\n", "\n"), searchResult.LyricText.Replace("\r\n", "\n"));
            Assert.False(searchResult.Instrumental);
        }

        private static void AssertNoDataFound(SearchResult searchResult)
        {
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.Equal(ExternalProviderType.Genius, searchResult.ExternalProviderType);
            Assert.False(searchResult.Instrumental);
        }
    }
}
