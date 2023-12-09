using LyricsScraperNET.Providers.AZLyrics;
using System;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.AZLyrics
{
    public class AZLyricsUriConverterTests
    {
        [Theory]
        [InlineData("The Devil Wears Prada", "You Can't Spell Crap Without 'C'", "http://www.azlyrics.com/lyrics/devilwearsprada/youcantspellcrapwithoutc.html")]
        [InlineData(" Young Thug ", " Rich Nigga Shit ", "http://www.azlyrics.com/lyrics/youngthug/richniggashit.html")]
        public void GetLyricUri_MultipleInputs_ShouldBeParse(string artistName, string songName, string expectedUri)
        {
            // Arrange
            var uriConverter = new AZLyricsUriConverter();

            // Act
            var actual = uriConverter.GetLyricUri(artistName, songName);

            // Assert
            Assert.Equal(new Uri(expectedUri), actual);
        }
    }
}
