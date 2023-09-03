using Xunit;

namespace LyricsScraperNET.UnitTest.Extensions
{
    public class LyricsScraperClientExtensionsTest
    {
        private ILyricsScraperClient _lyricsScraperClient => new LyricsScraperClient();

        [Fact]
        public void LyricsScraperClient_WithAZLyrics_ReturnsIsEnabled()
        {
            var lyricsScraperClient = _lyricsScraperClient.WithAZLyrics();
            Assert.NotNull(lyricsScraperClient);
            Assert.True(lyricsScraperClient.IsEnabled);
        }

        [Fact]
        public void LyricsScraperClient_WithGenius_ReturnsIsEnabled()
        {
            var lyricsScraperClient = _lyricsScraperClient.WithGenius();
            Assert.NotNull(lyricsScraperClient);
            Assert.True(lyricsScraperClient.IsEnabled);
        }

        [Fact]
        public void LyricsScraperClient_WithMusixmatch_ReturnsIsEnabled()
        {
            var lyricsScraperClient = _lyricsScraperClient.WithMusixmatch();
            Assert.NotNull(lyricsScraperClient);
            Assert.True(lyricsScraperClient.IsEnabled);
        }

        [Fact]
        public void LyricsScraperClient_WithSongLyrics_ReturnsIsEnabled()
        {
            var lyricsScraperClient = _lyricsScraperClient.WithSongLyrics();
            Assert.NotNull(lyricsScraperClient);
            Assert.True(lyricsScraperClient.IsEnabled);
        }
    }
}
