using LyricsScraperNET.Providers.Genius;
using System;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.Genius
{
    public class GeniusUriConverterTests
    {
        [Theory]
        [InlineData("Parkway Drive", "Carrion", "https://genius.com/api/search?q=Parkway Drive Carrion")]
        public void GetLyricUri_MultipleInputs_ShouldBeParse(string artistName, string songName, string expectedUri)
        {
            // Arrange
            var uriConverter = new GeniusUriConverter();

            // Act
            var actual = uriConverter.GetLyricUri(artistName, songName);

            // Assert
            Assert.Equal(new Uri(expectedUri), actual);
        }
    }
}
