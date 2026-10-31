using LyricsScraperNET.Models.Responses;
using Xunit;

namespace LyricsScraperNET.UnitTest.Models.Responses
{
    public class ProviderAttemptOutcomeTests : ExtensibleEnumTests<ProviderAttemptOutcome>
    {
        protected override ProviderAttemptOutcome Create(string value) => new ProviderAttemptOutcome(value);

        [Theory]
        [MemberData(nameof(Outcomes))]
        public void IsAnswer_ForEveryOutcome(ProviderAttemptOutcome outcome, bool isAnswer)
        {
            // Act & Assert
            Assert.Equal(isAnswer, outcome.IsAnswer);
        }

        public static TheoryData<ProviderAttemptOutcome, bool> Outcomes() => new()
        {
            { ProviderAttemptOutcome.None, false },
            { ProviderAttemptOutcome.Found, true },
            { ProviderAttemptOutcome.Instrumental, true },
            { ProviderAttemptOutcome.NotFound, true },
            { ProviderAttemptOutcome.Failed, false },
            { ProviderAttemptOutcome.Timeout, false },
            { ProviderAttemptOutcome.Error, false },
            { ProviderAttemptOutcome.Canceled, false },
            { new ProviderAttemptOutcome("AddedInAFutureVersion"), false },
        };
    }
}
