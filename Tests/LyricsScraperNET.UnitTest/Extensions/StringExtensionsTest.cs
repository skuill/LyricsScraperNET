using LyricsScraperNET.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LyricsScraperNET.UnitTest.Extensions
{
    [TestClass]
    public class StringExtensionsTest
    {
        [DataTestMethod]
        [DataRow("Attack Attack!", "attack-attack!")]
        [DataRow("I Swear I'll Change", "i-swear-i-ll-change")]
        [DataRow("You Can't Spell Crap Without \"C\"", "you-can-t-spell-crap-without-c")]
        [DataRow("", "")]
        [DataRow(null, null)]
        public void СonvertToDashedFormat_MultipleInputs_ShouldBeParse(string input, string expected)
        {
            // Act
            var actual = StringExtensions.СonvertToDashedFormat(input);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [DataTestMethod]
        [DataRow("Attack Attack!", true, "attackattack")]
        [DataRow("The Devil Wears Prada", true, "devilwearsprada")]
        [DataRow("You Can't Spell Crap Without 'C'", false, "youcantspellcrapwithoutc")]
        [DataRow("Sound The Alarm", false, "soundthealarm")]
        [DataRow("", true, "")]
        [DataRow(null, true, null)]
        public void StripRedundantChars_MultipleInputs_ShouldBeParse(string input, bool removeArticles, string expected)
        {
            // Act
            var actual = StringExtensions.StripRedundantChars(input, removeArticles);

            // Assert
            Assert.AreEqual(expected, actual);
        }


        [DataTestMethod]
        [DataRow("<div>test</div>", "test")]
        [DataRow("This text < is for fun", "This text < is for fun")]
        [DataRow(">not deleted<", ">not deleted<")]
        [DataRow("", "")]
        [DataRow(null, null)]
        public void RemoveHtmlTags_MultipleInputs_ShouldBeParse(string input, string expected)
        {
            // Act
            var actual = StringExtensions.RemoveHtmlTags(input);

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}
