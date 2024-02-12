using LyricsScraperNET.Common;
using LyricsScraperNET.Models.Requests;
using System;
using Xunit;

namespace LyricsScraperNET.UnitTest.Models.Requests
{
    public class UriSearchRequestTests
    {
        [Fact]
        public void IsValid_EmptyUri_ShouldBeFalse()
        {
            // Arrange
            Uri someUri = null;
            var searchRequest = new UriSearchRequest(someUri);

            // Act
            var validationResult = searchRequest.IsValid(out var errorMessage);

            // Assert
            Assert.False(validationResult);
            Assert.Equal(Constants.ResponseMessages.UriSearchRequestFieldsAreEmpty, errorMessage);
        }

        [Fact]
        public void IsValid_DefaultUri_ShouldBeTrue()
        {
            // Arrange
            var searchRequest = new UriSearchRequest("https://www.greatsitefortest111.com/");

            // Act
            var validationResult = searchRequest.IsValid(out var errorMessage);

            // Assert
            Assert.True(validationResult);
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }
    }
}
