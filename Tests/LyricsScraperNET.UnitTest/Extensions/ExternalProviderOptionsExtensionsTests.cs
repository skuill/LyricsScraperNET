using LyricsScraperNET.Extensions;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Genius;
using Xunit;

namespace LyricsScraperNET.UnitTest.Extensions
{
    public class ExternalProviderOptionsExtensionsTests
    {
        [Fact]
        public void TryGetApiKeyFromOptions_ShouldReturnFalse_WhenOptionsIsNull()
        {
            // Arrange
            IExternalProviderOptions options = null;

            // Act
            var result = options.TryGetApiKeyFromOptions(out var apiKey);

            // Assert
            Assert.False(result);
            Assert.Empty(apiKey);
        }

        [Fact]
        public void TryGetApiKeyFromOptions_ShouldReturnFalse_WhenOptionsDoesNotImplementIExternalProviderOptionsWithApiKey()
        {
            // Arrange
            var options = new AZLyricsOptions();

            // Act
            var result = options.TryGetApiKeyFromOptions(out var apiKey);

            // Assert
            Assert.False(result);
            Assert.Empty(apiKey);
        }

        [Fact]
        public void TryGetApiKeyFromOptions_ShouldReturnFalse_WhenApiKeyIsNullOrWhitespace()
        {
            // Arrange
            var options = new GeniusOptions { ApiKey = "   " };

            // Act
            var result = options.TryGetApiKeyFromOptions(out var apiKey);

            // Assert
            Assert.False(result);
            Assert.Empty(apiKey);
        }

        [Fact]
        public void TryGetApiKeyFromOptions_ShouldReturnTrueAndSetApiKey_WhenApiKeyIsValid()
        {
            // Arrange
            var expectedApiKey = "ValidApiKey";
            var options = new GeniusOptions { ApiKey = expectedApiKey };

            // Act
            var result = options.TryGetApiKeyFromOptions(out var apiKey);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedApiKey, apiKey);
        }
    }
}
