using LyricsScraperNET.Providers.LyricFind;
using System;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.LyricFind
{
    public class LyricFindUriConverterTests
    {
        [Theory]
        [InlineData("Bryan Adams", "Summer of '69", "https://lyrics.lyricfind.com/lyrics/bryan-adams-summer-of-69")]
        [InlineData("Patty & the Emblems", "Mixed-Up, Shook-Up Girl", "https://lyrics.lyricfind.com/lyrics/patty-the-emblems-mixed-up-shook-up-girl")]
        [InlineData("Mac DeMarco", "Me and Jon, Hanging On", "https://lyrics.lyricfind.com/lyrics/mac-demarco-me-and-jon-hanging-on")]
        [InlineData("Of Mice & Men", "You're Not Alone", "https://lyrics.lyricfind.com/lyrics/of-mice-men-youre-not-alone")]
        [InlineData("Mac DeMarco", "106.2 Breeze FM", "https://lyrics.lyricfind.com/lyrics/mac-demarco-106-2-breeze-fm")]
        [InlineData("Of Mice & Men", "O.C. Loko", "https://lyrics.lyricfind.com/lyrics/of-mice-men-o-c-loko")]
        [InlineData("Attack Attack!", "\"I Swear I'll Change\"", "https://lyrics.lyricfind.com/lyrics/attack-attack-i-swear-ill-change")]
        [InlineData("The Devil Wears Prada", "You Can't Spell \"Crap\" Without \"C\"", "https://lyrics.lyricfind.com/lyrics/the-devil-wears-prada-you-cant-spell-crap-without-c")]
        public void GetLyricUri_MultipleInputs_ShouldBeParse(string artistName, string songName, string expectedUri)
        {
            // Arrange
            var uriConverter = new LyricFindUriConverter();

            // Act
            var actual = uriConverter.GetLyricUri(artistName, songName);

            // Assert
            Assert.Equal(new Uri(expectedUri), actual);
        }
    }
}
