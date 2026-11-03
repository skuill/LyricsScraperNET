using LyricsScraperNET.Providers.LyricsFreak;
using System;
using System.Linq;
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
        [InlineData("Zé Ramalho", "https://www.lyricsfreak.com/z/ze+ramalho")]
        [InlineData("Yö", "https://www.lyricsfreak.com/y/yo")]
        [InlineData("U-ka Saegusa In Db", "https://www.lyricsfreak.com/u/u+ka+saegusa+in+db")]
        [InlineData(" Too $hort ", "https://www.lyricsfreak.com/t/too+hort")]
        [InlineData("É o Tchan", "https://www.lyricsfreak.com/e/e+o+tchan")]
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

        [Theory]
        // Base letters first, then the slug with the letters dropped: the site uses /z/z+ramalho/ but /b/beyonce/ and /o/olafur+arnalds/.
        [InlineData("Zé Ramalho", "https://www.lyricsfreak.com/z/ze+ramalho", "https://www.lyricsfreak.com/z/z+ramalho")]
        [InlineData("Beyoncé", "https://www.lyricsfreak.com/b/beyonce", "https://www.lyricsfreak.com/b/beyonc")]
        [InlineData("Ólafur Arnalds", "https://www.lyricsfreak.com/o/olafur+arnalds", "https://www.lyricsfreak.com/l/lafur+arnalds")]
        [InlineData("É o Tchan", "https://www.lyricsfreak.com/e/e+o+tchan", "https://www.lyricsfreak.com/o/+o+tchan")]
        // No diacritics: one candidate.
        [InlineData("Parkway Drive", "https://www.lyricsfreak.com/p/parkway+drive")]
        // Letters without a base letter are dropped in both forms: one candidate.
        [InlineData("Mø", "https://www.lyricsfreak.com/m/m")]
        // Latin letters are kept, other scripts dropped.
        [InlineData("Кино Kino", "https://www.lyricsfreak.com/k/+kino")]
        public void GetArtistUris_MultipleInputs_ReturnsCandidatesInOrder(string artistName, params string[] expectedUris)
        {
            // Arrange
            var uriConverter = new LyricsFreakUriConverter();

            // Act
            var actual = uriConverter.GetArtistUris(artistName);

            // Assert
            Assert.Equal(expectedUris.Select(uri => new Uri(uri)), actual);
        }

        [Theory]
        [InlineData("Кино")]
        [InlineData("방탄소년단")]
        [InlineData("!!!")]
        [InlineData("")]
        public void GetArtistUris_NoLatinLettersOrDigits_ReturnsEmpty(string artistName)
        {
            // Arrange
            var uriConverter = new LyricsFreakUriConverter();

            // Act
            var actual = uriConverter.GetArtistUris(artistName);

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public void GetArtistUri_NoLatinLettersOrDigits_ThrowsArgumentException()
        {
            // Arrange
            var uriConverter = new LyricsFreakUriConverter();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => uriConverter.GetArtistUri("Кино"));
        }
    }
}
