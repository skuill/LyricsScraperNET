using LyricsScraperNET.Configuration;
using Xunit;

namespace LyricsScraperNET.UnitTest.Configuration
{
    public class LyricScraperClientConfigTest
    {
        [Fact]
        public void LyricScraperClientConfig_IsEnabled_ReturnsFalseByDefault()
        {
            // Act
            var lyricScraperClientConfig = new LyricScraperClientConfig();

            // Assert
            Assert.False(lyricScraperClientConfig.IsEnabled);
        }

        [Fact]
        public void LyricScraperClientConfig_UseParallelSearch_ReturnsFalseByDefault()
        {
            // Act
            var lyricScraperClientConfig = new LyricScraperClientConfig();

            // Assert
            Assert.False(lyricScraperClientConfig.UseParallelSearch);
        }
    }
}
