using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.Providers.Musixmatch;
using LyricsScraperNET.TestShared.Providers;
using Moq;
using MusixmatchClientLib.API.Model.Exceptions;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.Musixmatch
{
    public class MusixmatchProviderTest : ProviderTestBase
    {
        [Fact]
        public void SearchLyric_ThrowAuthFailedException_ShouldRegenerateToken()
        {
            // Arrange
            string expectedLyric = "Жидкий кот, У ворот, мур-мур-мур, Он поёт.";
            var searchRequest = new ArtistAndSongSearchRequest("Дымка", "Мау");

            var clientWrapperMock = new Mock<IMusixmatchClientWrapper>();

            // First call. Throw an exception if the token is invalid or expired.
            clientWrapperMock.Setup(c => c.SearchLyric(It.IsAny<string>(), It.IsAny<string>(), It.Is<bool>(r => r == false)))
                .Returns(() => throw new MusixmatchRequestException(MusixmatchClientLib.API.Model.Types.StatusCode.AuthFailed));

            // Second call. A repeated call with token regeneration is expected.
            clientWrapperMock.Setup(c => c.SearchLyric(It.IsAny<string>(), It.IsAny<string>(), It.Is<bool>(r => r == true)))
                .Returns(new SearchResult(expectedLyric, ExternalProviderType.Musixmatch));

            var options = new MusixmatchOptions() { Enabled = true };
            var lyricsClient = new MusixmatchProvider(null, options, clientWrapperMock.Object);

            var result = lyricsClient.SearchLyric(searchRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedLyric, result.LyricText);
            clientWrapperMock.Verify(c => c.SearchLyric(It.IsAny<string>(), It.IsAny<string>(), It.Is<bool>(r => r == false)), Times.Once);
            clientWrapperMock.Verify(c => c.SearchLyric(It.IsAny<string>(), It.IsAny<string>(), It.Is<bool>(r => r == true)), Times.Once);
        }
    }
}
