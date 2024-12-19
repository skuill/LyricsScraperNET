using FakeItEasy;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.Providers.Musixmatch;
using LyricsScraperNET.TestShared.Providers;
using MusixmatchClientLib.API.Model.Exceptions;
using System.Threading;
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

            var clientWrapperFake = A.Fake<IMusixmatchClientWrapper>();

            // Первая попытка: выбрасывается исключение, имитирующее ошибку авторизации.
            A.CallTo(() => clientWrapperFake.SearchLyric(A<string>._, A<string>._, A<CancellationToken>._, A<bool>.That.Matches(x => x == false)))
                .Throws(new MusixmatchRequestException(MusixmatchClientLib.API.Model.Types.StatusCode.AuthFailed));

            // Вторая попытка: вызов с обновлённым токеном возвращает ожидаемый результат.
            A.CallTo(() => clientWrapperFake.SearchLyric(A<string>._, A<string>._, A<CancellationToken>._, A<bool>.That.Matches(x => x == true)))
                .Returns(new SearchResult(expectedLyric, ExternalProviderType.Musixmatch));

            var options = new MusixmatchOptions() { Enabled = true };
            var lyricsClient = new MusixmatchProvider(null, options, clientWrapperFake);

            // Act
            CancellationToken cancellationToken = new CancellationToken();
            var result = lyricsClient.SearchLyric(searchRequest, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedLyric, result.LyricText);
            A.CallTo(() => clientWrapperFake.SearchLyric(A<string>._, A<string>._, A<CancellationToken>._, A<bool>.That.Matches(x => x == false))).MustHaveHappenedOnceExactly();
            A.CallTo(() => clientWrapperFake.SearchLyric(A<string>._, A<string>.Ignored, A<CancellationToken>._, A<bool>.That.Matches(x => x == true))).MustHaveHappenedOnceExactly();
        }
    }
}
