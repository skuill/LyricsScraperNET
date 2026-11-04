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
        public void ConvertToDashedFormat_MultipleInputs_ShouldBeParse(string input, string expected)
        {
            // Act
            var actual = StringExtensions.ConvertToDashedFormat(input);

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
        [InlineData("Beyoncé", "Beyonce")]
        [InlineData("Motörhead", "Motorhead")]
        [InlineData("Zé Ramalho / Avôhai", "Ze Ramalho / Avohai")]
        [InlineData("Ólafur Arnalds", "Olafur Arnalds")]
        [InlineData("JAŸ-Z", "JAY-Z")]
        // Composed and decomposed forms give the same result.
        [InlineData("Béyoncé", "Beyonce")]
        // Letters without a decomposition and other scripts stay as they are.
        [InlineData("Sigur Rós, Mø, Straße", "Sigur Ros, Mø, Straße")]
        [InlineData("Группа крови", "Группа крови")]
        [InlineData("Attack Attack!", "Attack Attack!")]
        [InlineData("", "")]
        [InlineData(null, null)]
        public void RemoveDiacritics_MultipleInputs_ShouldBeParse(string input, string expected)
        {
            // Act
            var actual = StringExtensions.RemoveDiacritics(input);

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
        [InlineData("Attack Attack!", "attack+attack")]
        [InlineData("I Swear I'll Change", "i+swear+ill+change")]
        [InlineData("Summer of '69", "summer+of+69")]
        [InlineData(" Of Mice & Men ", "of+mice+men")]
        [InlineData("", "")]
        [InlineData(null, null)]
        public void ConvertToPlusFormat_MultipleInputs_ShouldBeParse(string input, string expected)
        {
            // Act
            var actual = StringExtensions.ConvertToPlusFormat(input, true);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
