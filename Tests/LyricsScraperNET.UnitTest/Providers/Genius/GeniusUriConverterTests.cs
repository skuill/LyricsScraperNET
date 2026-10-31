using LyricsScraperNET.Providers.Genius;
using System;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.Genius
{
    public class GeniusUriConverterTests
    {
        [Theory]
        [InlineData("Parkway Drive", "Carrion", "https://genius.com/api/search?q=Parkway%20Drive%20Carrion")]
        [InlineData("AC/DC", "Rock & Roll Ain't Noise Pollution", "https://genius.com/api/search?q=AC%2FDC%20Rock%20%26%20Roll%20Ain%27t%20Noise%20Pollution")]
        [InlineData("Attack Attack!", "What Happens If I Can't Check My MySpace When We Get There?", "https://genius.com/api/search?q=Attack%20Attack%21%20What%20Happens%20If%20I%20Can%27t%20Check%20My%20MySpace%20When%20We%20Get%20There%3F")]
        [InlineData("Artist #1", "Song=1+2", "https://genius.com/api/search?q=Artist%20%231%20Song%3D1%2B2")]
        [InlineData("Кино", "Группа крови", "https://genius.com/api/search?q=%D0%9A%D0%B8%D0%BD%D0%BE%20%D0%93%D1%80%D1%83%D0%BF%D0%BF%D0%B0%20%D0%BA%D1%80%D0%BE%D0%B2%D0%B8")]
        public void GetLyricUri_MultipleInputs_EscapesSearchQuery(string artistName, string songName, string expectedUri)
        {
            // Arrange
            var uriConverter = new GeniusUriConverter();

            // Act
            var actual = uriConverter.GetLyricUri(artistName, songName);

            // Assert
            Assert.Equal(expectedUri, actual.AbsoluteUri);
        }

        [Theory]
        [InlineData("Parkway Drive", "Carrion", "https://api.genius.com/search?q=Parkway%20Drive%20Carrion")]
        [InlineData("AC/DC", "Rock & Roll Ain't Noise Pollution", "https://api.genius.com/search?q=AC%2FDC%20Rock%20%26%20Roll%20Ain%27t%20Noise%20Pollution")]
        public void GetApiSearchUri_MultipleInputs_EscapesSearchQuery(string artistName, string songName, string expectedUri)
        {
            // Arrange
            var uriConverter = new GeniusUriConverter();

            // Act
            var actual = uriConverter.GetApiSearchUri(artistName, songName);

            // Assert
            Assert.Equal(expectedUri, actual.AbsoluteUri);
            Assert.Equal(GeniusUriConverter.ApiHost, actual.Host);
        }
    }
}
