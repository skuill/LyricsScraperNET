using LyricsScraperNET.Providers.LyricsFreak;
using System;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.LyricsFreak
{
    public class LyricsFreakUriConverterTests
    {
        [Theory]
        [InlineData("Bryan Adams", "https://www.lyricsfreak.com/b/bryan+adams")]
        [InlineData("Peter, Paul & Mary", "https://www.lyricsfreak.com/p/peter+paul+mary")]
        [InlineData("Mac DeMarco", "https://www.lyricsfreak.com/m/mac+demarco")]
        [InlineData("Of Mice & Men", "https://www.lyricsfreak.com/o/of+mice+men")]
        [InlineData("Attack Attack!", "https://www.lyricsfreak.com/a/attack+attack")]
        [InlineData("Zé Ramalho", "https://www.lyricsfreak.com/z/z+ramalho")]
        [InlineData("Yö", "https://www.lyricsfreak.com/y/y")]
        [InlineData("U-ka Saegusa In Db", "https://www.lyricsfreak.com/u/u+ka+saegusa+in+db")]
        [InlineData(" Too $hort ", "https://www.lyricsfreak.com/t/too+hort")]
        [InlineData("É o Tchan", "https://www.lyricsfreak.com/o/+o+tchan")]
        [InlineData("\"O Brother, Where Art Thou?\"", "https://www.lyricsfreak.com/o/o+brother+where+art+thou")]
        [InlineData("O.A.R. (Of A Revolution)", "https://www.lyricsfreak.com/o/oar+of+a+revolution")]
        [InlineData("Front 242", "https://www.lyricsfreak.com/f/front+242")]
        [InlineData("D12", "https://www.lyricsfreak.com/d/d12")]
        public void GetArtistUri_MultipleInputs_ShouldBeParse(string artistName, string expectedUri)
        {
            // Arrange
            var uriConverter = new LyricsFreakUriConverter();

            // Act
            var actual = uriConverter.GetArtistUri(artistName);

            // Assert
            Assert.Equal(new Uri(expectedUri), actual);
        }
    }
}
