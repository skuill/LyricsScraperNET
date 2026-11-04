using System;
using LyricsScraperNET.Providers.KPopLyrics;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.KPopLyrics
{
    public class KPopLyricsUriConverterTests
    {
        private const string SearchUriFormat = "https://www.kpoplyrics.net/wp-json/wp/v2/posts?search={0}&orderby=relevance&per_page=20&_fields=title,link";

        [Theory]
        [InlineData("JENNIE", "Mantra", "JENNIE%20Mantra")]
        [InlineData("JENNIE (제니)", "Mantra", "JENNIE%20Mantra")]
        [InlineData("or& (오아랜)", "Invitation to me (나에게로의 초대)", "or%20Invitation%20to%20me")]
        [InlineData("NINE.i (나인아이)", "Back to Christmas (크리스마스처럼)", "NINE%20i%20Back%20to%20Christmas")]
        [InlineData("ENHYPEN (엔하이픈)", "XO (Only If You Say Yes)", "ENHYPEN%20XO%20Only%20If%20You%20Say%20Yes")]
        [InlineData("(G)I-DLE", "Klaxon", "G%20I%20DLE%20Klaxon")]
        [InlineData("CA$H KiD", "CHANEL No.5", "CA%20H%20KiD%20CHANEL%20No%205")]
        [InlineData("ROSÉ", "number one girl", "ROS%C3%89%20number%20one%20girl")]
        [InlineData("BTS 방탄소년단", "Dynamite", "BTS%20Dynamite")]
        public void GetLyricUri_MultipleInputs_SearchesLatinPart(string artistName, string songName, string expectedSearch)
        {
            // Arrange
            var uriConverter = new KPopLyricsUriConverter();

            // Act
            var actual = uriConverter.GetLyricUri(artistName, songName);

            // Assert
            Assert.Equal(string.Format(SearchUriFormat, expectedSearch), actual.AbsoluteUri);
        }

        [Theory]
        [InlineData("JENNIE (제니)", "JENNIE")]
        [InlineData("Invitation to me (나에게로의 초대)", "Invitation to me")]
        [InlineData("XO (Only If You Say Yes)", "XO (Only If You Say Yes)")]
        [InlineData("Dynamite (Holiday Remix)", "Dynamite (Holiday Remix)")]
        [InlineData("Part (2)", "Part (2)")]
        [InlineData("(G)I-DLE", "(G)I-DLE")]
        [InlineData("방탄소년단", "방탄소년단")]
        public void GetTitleName_MultipleInputs_RemovesParenthesesWithoutLatinLetters(string name, string expected)
        {
            // Act
            var actual = KPopLyricsUriConverter.GetTitleName(name);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("BTS", true)]
        [InlineData("2NE1", true)]
        [InlineData("ROSÉ", true)]
        [InlineData("방탄소년단", false)]
        [InlineData("!!!", false)]
        [InlineData("", false)]
        public void HasLatinLetterOrDigit_MultipleInputs(string name, bool expected)
        {
            Assert.Equal(expected, KPopLyricsUriConverter.HasLatinLetterOrDigit(name));
        }

        [Fact]
        public void GetArtistUri_NotSupported_ThrowsNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() => new KPopLyricsUriConverter().GetArtistUri("BTS"));
        }
    }
}
