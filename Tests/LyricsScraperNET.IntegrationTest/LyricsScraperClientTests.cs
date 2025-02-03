using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using System.Threading.Tasks;
using Xunit;

namespace LyricsScraperNET.IntegrationTest
{
    public class LyricsScraperClientTests
    {
        [Fact]
        public void SearchLyric_WithDefaultExample_ShouldReturnSuccessfulSearchResult()
        {
            // Arrange
            string artistToSearch = "Parkway Drive";
            string songToSearch = "Idols And Anchors";
            ILyricsScraperClient lyricsScraperClient
                = new LyricsScraperClient()
                    .WithAllProviders();

            var searchRequest = new ArtistAndSongSearchRequest(artistToSearch, songToSearch);

            // Act
            var searchResult = lyricsScraperClient.SearchLyric(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.False(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.NotNull(searchResult.LyricText);
            Assert.False(searchResult.Instrumental);
        }

        [Fact]
        public async Task SearchLyricAsync_WithDefaultExample_ShouldReturnSuccessfulSearchResult()
        {
            // Arrange
            string artistToSearch = "Parkway Drive";
            string songToSearch = "Idols And Anchors";
            ILyricsScraperClient lyricsScraperClient
                = new LyricsScraperClient()
                    .WithAllProviders();

            var searchRequest = new ArtistAndSongSearchRequest(artistToSearch, songToSearch);

            // Act
            var searchResult = await lyricsScraperClient.SearchLyricAsync(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.False(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.ResponseMessage));
            Assert.NotNull(searchResult.LyricText);
            Assert.False(searchResult.Instrumental);
        }

        [Fact]
        public async Task SearchLyric_WithNotExistedLyric_ShouldReturnNotFound()
        {
            // Arrange
            string artistToSearch = "SomeNotExistedArtist12341235";
            string songToSearch = "SomeNotExistedLyric12341235";
            ILyricsScraperClient lyricsScraperClient
                = new LyricsScraperClient()
                    .WithAllProviders();

            var searchRequest = new ArtistAndSongSearchRequest(artistToSearch, songToSearch);

            // Act
            var searchResult = lyricsScraperClient.SearchLyric(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.LyricText));
            Assert.False(searchResult.Instrumental);
        }

        [Fact]
        public async Task SearchLyricAsync_WithNotExistedLyric_ShouldReturnNotFound()
        {
            // Arrange
            string artistToSearch = "SomeNotExistedArtist12341235";
            string songToSearch = "SomeNotExistedLyric12341235";
            ILyricsScraperClient lyricsScraperClient
                = new LyricsScraperClient()
                    .WithAllProviders();

            var searchRequest = new ArtistAndSongSearchRequest(artistToSearch, songToSearch);

            // Act
            var searchResult = await lyricsScraperClient.SearchLyricAsync(searchRequest);

            // Assert
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.Equal(ResponseStatusCode.NoDataFound, searchResult.ResponseStatusCode);
            Assert.True(string.IsNullOrEmpty(searchResult.LyricText));
            Assert.False(searchResult.Instrumental);
        }
    }
}
