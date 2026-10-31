using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace LyricsScraperNET.UnitTest.Models.Responses
{
    public class LyricsScraperModelFactoryTests
    {
        [Fact]
        public void SearchResult_AllValues_SetsThem()
        {
            // Arrange
            var attempts = new List<ProviderAttempt>
            {
                LyricsScraperModelFactory.ProviderAttempt(ExternalProviderType.Lrclib, ProviderAttemptOutcome.Failed, TimeSpan.FromMilliseconds(20), "HTTP 503"),
                LyricsScraperModelFactory.ProviderAttempt(ExternalProviderType.Genius, ProviderAttemptOutcome.Found, TimeSpan.FromMilliseconds(30)),
            };

            // Act
            var searchResult = LyricsScraperModelFactory.SearchResult(SearchResultCode.Found, "Lyrics", ExternalProviderType.Genius, "message", attempts);
            attempts.Clear();

            // Assert
            Assert.Equal(("Lyrics", ExternalProviderType.Genius, SearchResultCode.Found, "message", false),
                (searchResult.LyricText, searchResult.ExternalProviderType, searchResult.Code, searchResult.ResponseMessage, searchResult.Instrumental));
            Assert.Collection(searchResult.ProviderAttempts,
                attempt => Assert.Equal((ExternalProviderType.Lrclib, ProviderAttemptOutcome.Failed, TimeSpan.FromMilliseconds(20), "HTTP 503"),
                    (attempt.ProviderType, attempt.Outcome, attempt.Duration, attempt.Message)),
                attempt => Assert.Equal((ExternalProviderType.Genius, ProviderAttemptOutcome.Found, TimeSpan.FromMilliseconds(30), string.Empty),
                    (attempt.ProviderType, attempt.Outcome, attempt.Duration, attempt.Message)));
        }

        [Fact]
        public void SearchResult_Defaults_EmptyResultWithoutAttempts()
        {
            // Act
            var searchResult = LyricsScraperModelFactory.SearchResult(SearchResultCode.Instrumental, responseMessage: null);

            // Assert
            Assert.Equal((string.Empty, ExternalProviderType.None, SearchResultCode.Instrumental, string.Empty, true),
                (searchResult.LyricText, searchResult.ExternalProviderType, searchResult.Code, searchResult.ResponseMessage, searchResult.Instrumental));
            Assert.Empty(searchResult.ProviderAttempts);
        }
    }
}
