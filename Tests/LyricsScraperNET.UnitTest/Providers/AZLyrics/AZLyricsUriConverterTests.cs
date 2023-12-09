using LyricsScraperNET.Providers.AZLyrics;
using System;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.AZLyrics
{
    public class AZLyricsUriConverterTests
    {
        [Theory]
        [InlineData("The Devil Wears Prada", "You Can't Spell Crap Without 'C'", "http://www.azlyrics.com/lyrics/devilwearsprada/youcantspellcrapwithoutc.html")]
        [InlineData("The Devil Wears Prada", "You Can't Spell \"Crap\" Without \"C\"", "http://www.azlyrics.com/lyrics/devilwearsprada/youcantspellcrapwithoutc.html")]
        [InlineData(" Young Thug ", " Rich Nigga Shit ", "http://www.azlyrics.com/lyrics/youngthug/richniggashit.html")]
        [InlineData("Attack Attack!", "I Swear I'll Change", "http://www.azlyrics.com/lyrics/attackattack/iswearillchange.html")]
        [InlineData("Attack Attack!", "\"I Swear I'll Change\"", "http://www.azlyrics.com/lyrics/attackattack/iswearillchange.html")]
        [InlineData("Against Me!", "Stop!", "http://www.azlyrics.com/lyrics/againstme/stop.html")]
        [InlineData("Bryan Adams", "Summer of '69", "http://www.azlyrics.com/lyrics/bryanadams/summerof69.html")]
        [InlineData("Bob Dylan", "Leopard-Skin Pill-Box Hat", "http://www.azlyrics.com/lyrics/bobdylan/leopardskinpillboxhat.html")]
        [InlineData("Mac DeMarco", "Me and Jon, Hanging On", "http://www.azlyrics.com/lyrics/macdemarco/meandjonhangingon.html")]
        [InlineData("Of Mice & Men", "You're Not Alone", "http://www.azlyrics.com/lyrics/ofmicemen/yourenotalone.html")]
        [InlineData("Of Mice & Men", "O.G. Loko", "http://www.azlyrics.com/lyrics/ofmicemen/ogloko.html")]
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
