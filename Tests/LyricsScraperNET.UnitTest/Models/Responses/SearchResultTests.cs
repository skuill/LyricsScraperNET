using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Models;
using System;
using System.Text.Json;
using Xunit;

namespace LyricsScraperNET.UnitTest.Models.Responses
{
    public class SearchResultTests
    {
        [Theory]
        [MemberData(nameof(StatusCodes))]
        public void ResponseStatusCode_DerivedFromCode(SearchResultCode code, ResponseStatusCode expected)
        {
            // Arrange
            var searchResult = new SearchResult(string.Empty, ExternalProviderType.None, code);

            // Act & Assert
            Assert.Equal(expected, searchResult.ResponseStatusCode);
        }

        public static TheoryData<SearchResultCode, ResponseStatusCode> StatusCodes() => new()
        {
            { SearchResultCode.None, ResponseStatusCode.None },
            { SearchResultCode.Found, ResponseStatusCode.Success },
            { SearchResultCode.Instrumental, ResponseStatusCode.Success },
            { SearchResultCode.NotFound, ResponseStatusCode.NoDataFound },
            { SearchResultCode.NoProviders, ResponseStatusCode.NoDataFound },
            { SearchResultCode.AllProvidersDisabled, ResponseStatusCode.NoDataFound },
            { SearchResultCode.AllProvidersFailed, ResponseStatusCode.Error },
            { SearchResultCode.EmptyRequest, ResponseStatusCode.BadRequest },
            { SearchResultCode.EmptyArtistOrSong, ResponseStatusCode.BadRequest },
            { SearchResultCode.EmptyUri, ResponseStatusCode.BadRequest },
            { SearchResultCode.ProviderNotAvailable, ResponseStatusCode.BadRequest },
            { new SearchResultCode("AddedInAFutureVersion"), ResponseStatusCode.NoDataFound },
        };

        [Theory]
        [InlineData("Some lyrics", "Found")]
        [InlineData("", "NotFound")]
        [InlineData("   ", "NotFound")]
        public void Constructor_LyricText_SetsFoundOnlyForNonEmptyText(string lyricText, string expectedCode)
        {
            // Act
            var searchResult = new SearchResult(lyricText, ExternalProviderType.Genius);

            // Assert
            Assert.Equal(new SearchResultCode(expectedCode), searchResult.Code);
            Assert.Equal(ExternalProviderType.Genius, searchResult.ExternalProviderType);
            Assert.False(searchResult.Instrumental);
        }

        [Fact]
        public void Instrumental_DerivedFromCode()
        {
            // Act
            var searchResult = new SearchResult(string.Empty, ExternalProviderType.Lrclib, SearchResultCode.Instrumental);

            // Assert
            Assert.True(searchResult.Instrumental);
            Assert.True(searchResult.IsEmpty());
            Assert.True(searchResult.Code.IsSuccess);
        }

        [Fact]
        public void Empty_ReturnsNewNotFoundResultEachTime()
        {
            // Act
            var first = SearchResult.Empty;
            var second = SearchResult.Empty;

            // Assert
            Assert.NotSame(first, second);
            Assert.Equal(SearchResultCode.NotFound, first.Code);
            Assert.True(first.IsEmpty());
            Assert.Empty(first.ProviderAttempts);
        }

        [Fact]
        public void WithProviderAttempts_ReturnsCopyAndKeepsTheOriginal()
        {
            // Arrange
            var original = new SearchResult("Lyrics", ExternalProviderType.Genius) { ResponseMessage = "message" };
            var attempts = new[] { new ProviderAttempt(ExternalProviderType.Genius, ProviderAttemptOutcome.Found, TimeSpan.FromMilliseconds(5)) };

            // Act
            var copy = original.WithProviderAttempts(attempts);

            // Assert
            Assert.NotSame(original, copy);
            Assert.Equal(("Lyrics", ExternalProviderType.Genius, SearchResultCode.Found, "message"), (copy.LyricText, copy.ExternalProviderType, copy.Code, copy.ResponseMessage));
            Assert.Same(attempts, copy.ProviderAttempts);
            Assert.Empty(original.ProviderAttempts);
        }

        [Fact]
        public void Json_WritesCodesAsStrings()
        {
            // Arrange
            var searchResult = new SearchResult(string.Empty, ExternalProviderType.None, SearchResultCode.AllProvidersFailed)
            {
                ProviderAttempts = new[] { new ProviderAttempt(ExternalProviderType.Genius, ProviderAttemptOutcome.Timeout, TimeSpan.FromSeconds(10), "No answer within 00:00:10") },
            };

            // Act
            using var json = JsonDocument.Parse(JsonSerializer.Serialize(searchResult));

            // Assert
            var root = json.RootElement;
            Assert.Equal("AllProvidersFailed", root.GetProperty("Code").GetString());
            var attempt = Assert.Single(root.GetProperty("ProviderAttempts").EnumerateArray());
            Assert.Equal("Timeout", attempt.GetProperty("Outcome").GetString());
            Assert.Equal((int)ExternalProviderType.Genius, attempt.GetProperty("ProviderType").GetInt32());
            Assert.Equal("00:00:10", attempt.GetProperty("Duration").GetString());
            Assert.Equal("No answer within 00:00:10", attempt.GetProperty("Message").GetString());
            Assert.False(root.TryGetProperty("LoadFailure", out _));
        }
    }
}
