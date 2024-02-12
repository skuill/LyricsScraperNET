using LyricsScraperNET.Common;
using LyricsScraperNET.Models.Requests;
using Xunit;

namespace LyricsScraperNET.UnitTest.Models.Requests
{
    public class ArtistAndSongSearchRequestTests
    {
        [Theory]
        [InlineData("", "")]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData(null, "")]
        [InlineData(null, "SomeSong")]
        [InlineData("SomeArtist", null)]
        public void IsValid_EmptyArtistAndSong_ShouldBeFalse(string artist, string song)
        {
            // Arrange
            var searchRequest = new ArtistAndSongSearchRequest(artist, song);

            // Act
            var validationResult = searchRequest.IsValid(out var errorMessage);

            // Assert
            Assert.False(validationResult);
            Assert.Equal(Constants.ResponseMessages.ArtistAndSongSearchRequestFieldsAreEmpty, errorMessage);
        }

        [Fact]
        public void IsValid_DefaultArtistAndSong_ShouldBeTrue()
        {
            // Arrange
            var searchRequest = new ArtistAndSongSearchRequest("SomeArtist", "SomeSong");

            // Act
            var validationResult = searchRequest.IsValid(out var errorMessage);

            // Assert
            Assert.True(validationResult);
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }
    }
}
