using LyricsScraperNET.Providers.LyricsFreak;
using System;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.LyricsFreak
{
    public class LyricsFreakUriConverterTests
    {
        [Theory]
        [InlineData("Bryan Adams", "Summer of '69", "https://www.lyricsfreak.com/b/bryan+adams")]
        [InlineData("Patty & the Emblems", "Mixed+Up, Shook+Up Girl", "https://www.lyricsfreak.com/p/patty+the+emblems")]
        [InlineData("Mac DeMarco", "Me and Jon, Hanging On", "https://www.lyricsfreak.com/m/mac+demarco")]
        [InlineData("Of Mice & Men", "You're Not Alone", "https://www.lyricsfreak.com/o/of+mice+men")]
        [InlineData("Attack Attack!", "I Swear I'll Change", "https://www.lyricsfreak.com/a/attack+attack")]
        public void GetLyricUri_MultipleInputs_ShouldBeParse(string artistName, string songName, string expectedUri)
        {  
            // Arrange
            var uriConverter = new LyricsFreakUriConverter();

            // Act
            var actual = uriConverter.GetLyricUri(artistName, songName);

            // Assert
            Assert.Equal(new Uri(expectedUri), actual);
        }
    }
}
