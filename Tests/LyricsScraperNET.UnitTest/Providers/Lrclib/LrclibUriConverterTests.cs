using LyricsScraperNET.Providers.Lrclib;
using System;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.Lrclib
{
    public class LrclibUriConverterTests
    {
        [Theory]
        [InlineData("Parkway Drive", "Idols and Anchors", "https://lrclib.net/api/get?artist_name=Parkway%20Drive&track_name=Idols%20and%20Anchors")]
        [InlineData("Borislav Slavov", "I Want to Live", "https://lrclib.net/api/get?artist_name=Borislav%20Slavov&track_name=I%20Want%20to%20Live")]
        [InlineData("Of Mice & Men", "You're Not Alone", "https://lrclib.net/api/get?artist_name=Of%20Mice%20%26%20Men&track_name=You%27re%20Not%20Alone")]
        public void GetLyricUri_MultipleInputs_ShouldBeParse(string artistName, string songName, string expectedUri)
        {
            // Arrange
            var uriConverter = new LrclibUriConverter();

            // Act
            var actual = uriConverter.GetLyricUri(artistName, songName);

            // Assert
            Assert.Equal(new Uri(expectedUri), actual);
        }
    }
}
