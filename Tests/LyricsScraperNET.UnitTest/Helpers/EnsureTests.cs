using LyricsScraperNET.Helpers;
using System;
using System.Collections.Generic;
using Xunit;

namespace LyricsScraperNET.UnitTest.Helpers
{
    public class EnsureTests
    {
        [Fact]
        public void ArgumentNotNull_ShouldNotThrow_WhenValueIsNotNull()
        {
            // Arrange
            var value = new object();

            // Act & Assert
            Ensure.ArgumentNotNull(value, nameof(value));
        }

        [Fact]
        public void ArgumentNotNull_ShouldThrowArgumentNullException_WhenValueIsNull()
        {
            // Arrange
            object value = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => Ensure.ArgumentNotNull(value, nameof(value)));
            Assert.Equal(nameof(value), exception.ParamName);
        }

        [Fact]
        public void ArgumentNotNullOrEmptyString_ShouldNotThrow_WhenValueIsValid()
        {
            // Arrange
            var value = "Valid string";

            // Act & Assert
            Ensure.ArgumentNotNullOrEmptyString(value, nameof(value));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ArgumentNotNullOrEmptyString_ShouldThrowArgumentException_WhenValueIsNullOrEmpty(string value)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => Ensure.ArgumentNotNullOrEmptyString(value, nameof(value)));
            Assert.Equal(nameof(value), exception.ParamName);
            Assert.Contains("empty or null", exception.Message);
        }

        [Fact]
        public void ArgumentNotNullOrEmptyList_ShouldNotThrow_WhenListIsValid()
        {
            // Arrange
            var value = new List<int> { 1, 2, 3 };

            // Act & Assert
            Ensure.ArgumentNotNullOrEmptyList(value, nameof(value));
        }

        [Fact]
        public void ArgumentNotNullOrEmptyList_ShouldThrowArgumentException_WhenListIsNull()
        {
            // Arrange
            List<int> value = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => Ensure.ArgumentNotNullOrEmptyList(value, nameof(value)));
            Assert.Equal(nameof(value), exception.ParamName);
            Assert.Contains("empty or null", exception.Message);
        }

        [Fact]
        public void ArgumentNotNullOrEmptyList_ShouldThrowArgumentException_WhenListIsEmpty()
        {
            // Arrange
            var value = new List<int>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => Ensure.ArgumentNotNullOrEmptyList(value, nameof(value)));
            Assert.Equal(nameof(value), exception.ParamName);
            Assert.Contains("empty or null", exception.Message);
        }
    }
}
