using LyricsScraperNET.Providers.SongLyrics;
using System;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.SongLyrics
{
    public class SongLyricsUriConverterTests
    {
        [Theory]
        [InlineData("Attack Attack!", "I Swear I'll Change", "https://www.songlyrics.com/attack-attack!/i-swear-i-ll-change-lyrics/")]
        [InlineData("Attack Attack!", "\"I Swear I'll Change\"", "https://www.songlyrics.com/attack-attack!/i-swear-i-ll-change-lyrics/")]
        [InlineData("Against Me!", "Stop!", "https://www.songlyrics.com/against-me!/stop!-lyrics/")]
        [InlineData("The Devil Wears Prada", "You Can't Spell Crap Without \"C\"", "https://www.songlyrics.com/the-devil-wears-prada/you-can-t-spell-crap-without-c-lyrics/")]
        [InlineData("The Devil Wears Prada", "You Can't Spell \"Crap\" Without \"C\"", "https://www.songlyrics.com/the-devil-wears-prada/you-can-t-spell-crap-without-c-lyrics/")]
        [InlineData("Bryan Adams", "Summer of '69", "https://www.songlyrics.com/bryan-adams/summer-of-69-lyrics/")]
        [InlineData("Patty & The Emblems", "Mixed Up Shook Up Girl (Long Version)", "https://www.songlyrics.com/patty-the-emblems/mixed-up-shook-up-girl-long-version-lyrics/")]
        [InlineData("Mac DeMarco", "Me and Jon, Hanging On", "https://www.songlyrics.com/mac-demarco/me-and-jon-hanging-on-lyrics/")]
        [InlineData("Of Mice & Men", "You're Not Alone", "https://www.songlyrics.com/of-mice-men/you-re-not-alone-lyrics/")]
        [InlineData("Mac DeMarco", "106.2 Breeze FM", "https://www.songlyrics.com/mac-demarco/106-2-breeze-fm-lyrics/")]
        [InlineData("Of Mice & Men", "O.G. Loko", "https://www.songlyrics.com/of-mice-men/o-g-loko-lyrics/")]
        public void GetLyricUri_MultipleInputs_ShouldBeParse(string artistName, string songName, string expectedUri)
        {
            // Arrange
            var uriConverter = new SongLyricsUriConverter();

            // Act
            var actual = uriConverter.GetLyricUri(artistName, songName);

            // Assert
            Assert.Equal(new Uri(expectedUri), actual);
        }
    }
}
