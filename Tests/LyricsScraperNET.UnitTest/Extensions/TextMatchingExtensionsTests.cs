using LyricsScraperNET.Extensions;
using Xunit;

namespace LyricsScraperNET.UnitTest.Extensions
{
    public class TextMatchingExtensionsTests
    {
        [Theory]
        [InlineData("Test Song", "Test Song")]
        [InlineData("TEST SONG", "test song")]
        [InlineData("Test Song", " Test   Song ")]
        [InlineData("Don’t Stop", "Don't Stop")]
        [InlineData("Don’t Stop", "Dont Stop")]
        [InlineData("'Til Dawn", "‘Til Dawn")]
        [InlineData("Rock and Roll", "Rock & Roll")]
        [InlineData("Beyoncé", "Beyonce")]
        [InlineData("JAŸ-Z", "Jay-Z")]
        [InlineData("Avôhai", "Avohai")]
        [InlineData("Sweet Child O' Mine", "Sweet Child O Mine")]
        [InlineData("Harder Better Faster Stronger", "Harder, Better, Faster, Stronger")]
        [InlineData("What Happens If I Can’t Check My MySpace When We Get There?", "What Happens If I Cant Check My MySpace When We Get There")]
        [InlineData("Dr. Shavargo, Pt. 3", "Dr. Shavargo Part 3")]
        [InlineData("!!!", "!!!")]
        [InlineData("Группа Крови", "Группа крови")]
        public void IsSameText_SameTextWrittenDifferently_ReturnsTrue(string actual, string expected)
        {
            // Act
            var result = actual.IsSameText(expected);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("Another Song", "Test Song")]
        [InlineData("Test Song (Live)", "Test Song")]
        [InlineData("Test Song", "Test Song (Remix)")]
        [InlineData("Around The World / Harder Better Faster Stronger", "Harder Better Faster Stronger")]
        [InlineData("!!!", "???")]
        [InlineData("", "Test Song")]
        [InlineData(null, "Test Song")]
        public void IsSameText_DifferentText_ReturnsFalse(string actual, string expected)
        {
            // Act
            var result = actual.IsSameText(expected);

            // Assert
            Assert.False(result);
        }
    }
}
