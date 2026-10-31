using LyricsScraperNET.Models.Responses;
using Xunit;

namespace LyricsScraperNET.UnitTest.Models.Responses
{
    public class SearchResultCodeTests : ExtensibleEnumTests<SearchResultCode>
    {
        protected override SearchResultCode Create(string value) => new SearchResultCode(value);

        [Theory]
        [MemberData(nameof(Codes))]
        public void IsSuccessAndIsInvalidRequestOrSetup_ForEveryCode(SearchResultCode code, bool isSuccess, bool isInvalidRequestOrSetup)
        {
            // Act & Assert
            Assert.Equal(isSuccess, code.IsSuccess);
            Assert.Equal(isInvalidRequestOrSetup, code.IsInvalidRequestOrSetup);
        }

        public static TheoryData<SearchResultCode, bool, bool> Codes() => new()
        {
            { SearchResultCode.None, false, false },
            { SearchResultCode.Found, true, false },
            { SearchResultCode.Instrumental, true, false },
            { SearchResultCode.NotFound, false, false },
            { SearchResultCode.AllProvidersFailed, false, false },
            { SearchResultCode.EmptyRequest, false, true },
            { SearchResultCode.EmptyArtistOrSong, false, true },
            { SearchResultCode.EmptyUri, false, true },
            { SearchResultCode.ProviderNotAvailable, false, true },
            { SearchResultCode.NoProviders, false, true },
            { SearchResultCode.AllProvidersDisabled, false, true },
            { new SearchResultCode("AddedInAFutureVersion"), false, false },
        };

        [Fact]
        public void Values_CanBeUsedInSwitch()
        {
            // Arrange
            var code = SearchResultCode.AllProvidersFailed;

            // Act
            var description = code.Value switch
            {
                SearchResultCode.Values.NotFound => "not found",
                SearchResultCode.Values.AllProvidersFailed => "retry later",
                _ => "other",
            };

            // Assert
            Assert.Equal("retry later", description);
        }
    }
}
