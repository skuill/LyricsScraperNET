using LyricsScraperNET.Common;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.TestShared.Helpers;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.Models
{
    public class ExternalProviderTypeTests
    {
        [Fact]
        public void NumericValues_ShouldNotChange()
        {
            // Consumers map this enum by number (for example LyricsScraperApi), so removing a provider must not shift the values.
            var expected = new Dictionary<string, int>
            {
                { "None", 0 },
                { "AZLyrics", 1 },
                { "Genius", 2 },
                { "Musixmatch", 3 },
                { "SongLyrics", 4 },
                { "LyricFind", 5 },
                { "KPopLyrics", 6 },
                { "LyricsFreak", 7 },
                { "Lrclib", 8 },
            };

            // Act
            var actual = System.Enum.GetValues<ExternalProviderType>()
                .ToDictionary(providerType => providerType.ToString(), providerType => (int)providerType);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RemovedProviderTypes_ShouldBeMusixmatchAndLyricFind()
        {
            // Arrange
#pragma warning disable CS0618 // Obsolete: the providers were removed in 3.0.0
            var expected = new[] { ExternalProviderType.Musixmatch, ExternalProviderType.LyricFind };
#pragma warning restore CS0618

            // Act
            var actual = ExternalProviderTypeHelper.RemovedProviderTypes;

            // Assert
            Assert.Equal(expected, actual);
            Assert.DoesNotContain(actual, providerType => Constants.ProvidersSearchPriorities.ContainsKey(providerType));
        }

        [Fact]
        public void SupportedProviderTypes_ShouldHaveSearchPriority()
        {
            // Act
            var actual = Constants.ProvidersSearchPriorities.Keys.OrderBy(x => x);

            // Assert
            Assert.Equal(ExternalProviderTypeHelper.SupportedProviderTypes.OrderBy(x => x), actual);
        }

        [Fact]
        public void SearchLyric_WithRemovedProviderType_ShouldReturnBadRequest()
        {
            // Arrange
            var lyricsScraperClient = new LyricsScraperClient().WithAllProviders();

            foreach (var providerType in ExternalProviderTypeHelper.RemovedProviderTypes)
            {
                var searchRequest = new ArtistAndSongSearchRequest("Parkway Drive", "Idols and Anchors", providerType);

                // Act
                var searchResult = lyricsScraperClient.SearchLyric(searchRequest, TestContext.Current.CancellationToken);

                // Assert
                Assert.Equal(ResponseStatusCode.BadRequest, searchResult.ResponseStatusCode);
                Assert.Equal(Constants.ResponseMessages.ExternalProviderForRequestNotSpecified, searchResult.ResponseMessage);
                Assert.Equal(ExternalProviderType.None, searchResult.ExternalProviderType);
                Assert.True(searchResult.IsEmpty());
                Assert.False(searchResult.Instrumental);
            }
        }
    }
}
