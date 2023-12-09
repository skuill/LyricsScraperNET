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
    }
}
