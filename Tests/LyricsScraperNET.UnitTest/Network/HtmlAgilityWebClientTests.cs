using LyricsScraperNET.Network;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LyricsScraperNET.UnitTest.Network
{
    public class HtmlAgilityWebClientTests
    {
        [Fact]
        public void Load_InvalidUri_ShouldReturnEmptyString()
        {
            // Arrange
            var webClient = new HtmlAgilityWebClient();
            var invalidUri = new Uri("http://nonexistent12345123.url");

            // Act
            var result = webClient.Load(invalidUri, CancellationToken.None);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task LoadAsync_InvalidUri_ShouldReturnEmptyString()
        {
            // Arrange
            var webClient = new HtmlAgilityWebClient();
            var invalidUri = new Uri("http://nonexistent12345123.url");

            // Act
            var result = await webClient.LoadAsync(invalidUri, CancellationToken.None);

            // Assert
            Assert.Equal(string.Empty, result);
        }
    }
}
