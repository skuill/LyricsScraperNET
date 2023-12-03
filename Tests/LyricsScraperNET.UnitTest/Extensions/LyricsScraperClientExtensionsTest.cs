using Xunit;

namespace LyricsScraperNET.UnitTest.Extensions
{
    public class LyricsScraperClientExtensionsTest
    {
        private ILyricsScraperClient _lyricsScraperClient => new LyricsScraperClient();

        [Fact]
        public void LyricsScraperClient_WithAZLyrics_ReturnsIsEnabled()
        {
            // Act
            var lyricsScraperClient = _lyricsScraperClient.WithAZLyrics();

            // Assert
            Assert.NotNull(lyricsScraperClient);
            Assert.True(lyricsScraperClient.IsEnabled);
        }

        [Fact]
        public void LyricsScraperClient_WithGenius_ReturnsIsEnabled()
        {
            // Act
            var lyricsScraperClient = _lyricsScraperClient.WithGenius();

            // Assert
            Assert.NotNull(lyricsScraperClient);
            Assert.True(lyricsScraperClient.IsEnabled);
        }

        [Fact]
        public void LyricsScraperClient_WithMusixmatch_ReturnsIsEnabled()
        {
            // Act
            var lyricsScraperClient = _lyricsScraperClient.WithMusixmatch();

            // Assert
            Assert.NotNull(lyricsScraperClient);
            Assert.True(lyricsScraperClient.IsEnabled);
        }

        [Fact]
        public void LyricsScraperClient_WithSongLyrics_ReturnsIsEnabled()
        {
            // Act
            var lyricsScraperClient = _lyricsScraperClient.WithSongLyrics();

            // Assert
            Assert.NotNull(lyricsScraperClient);
            Assert.True(lyricsScraperClient.IsEnabled);
        }
    }
}
