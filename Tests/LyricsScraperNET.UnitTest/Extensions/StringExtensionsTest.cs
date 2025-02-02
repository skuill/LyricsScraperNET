using LyricsScraperNET.Extensions;
using Xunit;

namespace LyricsScraperNET.UnitTest.Extensions
{
    public class StringExtensionsTest
    {
        [Theory]
        [InlineData("Attack Attack!", "attack-attack!")]
        [InlineData("I Swear I'll Change", "i-swear-i-ll-change")]
        [InlineData("You Can't Spell Crap Without \"C\"", "you-can-t-spell-crap-without-c")]
        [InlineData("Summer of '69", "summer-of-69")]
        [InlineData(" Of Mice & Men ", "of-mice-men")]
        [InlineData("", "")]
        [InlineData(null, null)]
        public void СonvertToDashedFormat_MultipleInputs_ShouldBeParse(string input, string expected)
        {
            // Act
            var actual = StringExtensions.СonvertToDashedFormat(input);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("Attack Attack!", true, "attackattack")]
        [InlineData("The Devil Wears Prada", true, "devilwearsprada")]
        [InlineData("You Can't Spell Crap Without 'C'", false, "youcantspellcrapwithoutc")]
        [InlineData("Sound The Alarm", false, "soundthealarm")]
        [InlineData("", true, "")]
        [InlineData(null, true, null)]
        public void StripRedundantChars_MultipleInputs_ShouldBeParse(string input, bool removeArticles, string expected)
        {
            // Act
            var actual = StringExtensions.StripRedundantChars(input, removeArticles);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("<div>test</div>", "test")]
        [InlineData("This text < is for fun", "This text < is for fun")]
        [InlineData("> not deleted<", "> not deleted<")]
        [InlineData("", "")]
        [InlineData(null, null)]
        public void RemoveHtmlTags_MultipleInputs_ShouldBeParse(string input, string expected)
        {
            // Act
            var actual = StringExtensions.RemoveHtmlTags(input);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("&some random", "test", "some-random-test")] // Removes `&`
        [InlineData("*some random", "test", "some-random-test")] // Removes `*`
        [InlineData("(some) random", "test", "some-random-test")] // DONT strip ASCII content in parentheses
        [InlineData("(엔하이픈) random", "test", "random-test")] // strip NON-ASCII content in parentheses
        [InlineData("엔하이픈 random", "test", "random-test")] // Non-ASCII handling
        [InlineData("", "test", "test")] // Empty artist
        [InlineData("artist", "", "artist")] // Empty title
        [InlineData("", "", "")] // Both empty
        [InlineData("   ", "test", "test")] // Artist with spaces only
        [InlineData("artist", "   ", "artist")] // Title with spaces only
        [InlineData("A Very Long Artist Name", "With CAPS", "a-very-long-artist-name-with-caps")] // Mixed case
        [InlineData("artist---name", "title!!!", "artist-name-title")] // Multiple special characters
        [InlineData("trailing-", "-leading", "trailing-leading")] // Ensure no trailing/leading dashes
        public void GenerateCombinedUrlSlug_Tests(string artist, string title, string expected)
        {
            var slug = StringExtensions.CreateCombinedUrlSlug(artist, title);

            Assert.Equal(expected, slug);
        }

        [Theory]
        [InlineData("Attack Attack!", "attack+attack")]
        [InlineData("I Swear I'll Change", "i+swear+ill+change")]
        [InlineData("Summer of '69", "summer+of+69")]
        [InlineData(" Of Mice & Men ", "of+mice+men")]
        [InlineData("", "")]
        [InlineData(null, null)]
        public void СonvertToPlusFormat_MultipleInputs_ShouldBeParse(string input, string expected)
        {
            // Act
            var actual = StringExtensions.СonvertToPlusFormat(input, true);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
