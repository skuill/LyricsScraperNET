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
        public void СonvertToDashedFormat_MultipleInputs_ShouldBeParse(string input, string expected)
        {
            // Act
            var actual = StringExtensions.СonvertToDashedFormat(input);

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}
