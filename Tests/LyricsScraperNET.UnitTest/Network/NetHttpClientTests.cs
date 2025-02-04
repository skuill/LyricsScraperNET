using LyricsScraperNET.Network;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LyricsScraperNET.UnitTest.Network
{
    public class NetHttpClientTests
    {
        [Fact]
        public void Load_InvalidUri_ShouldReturnEmptyString()
        {
            // Arrange
            var webClient = new NetHttpClient();
            var invalidUri = new Uri("http://nonexistent12345123.url");

            // Act
            var result = webClient.Load(invalidUri, CancellationToken.None);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Load_CancelledToken_ShouldThrowException()
        {
            // Arrange
            var webClient = new NetHttpClient();
            var invalidUri = new Uri("http://nonexistent12345123.url");
            var token = GetCancelledCancellationToken();

            // Act & Assert
            Assert.Throws<TaskCanceledException>(() => webClient.Load(invalidUri, token));
        }

        [Fact]
        public async Task LoadAsync_InvalidUri_ShouldReturnEmptyString()
        {
            // Arrange
            var webClient = new NetHttpClient();
            var invalidUri = new Uri("http://nonexistent12345123.url");

            // Act
            var result = await webClient.LoadAsync(invalidUri, CancellationToken.None);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task LoadAsync_CancelledToken_ShouldThrowException()
        {
            // Arrange
            var webClient = new NetHttpClient();
            var invalidUri = new Uri("http://nonexistent12345123.url");
            var token = GetCancelledCancellationToken();

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(async () => await webClient.LoadAsync(invalidUri, token));
        }

        private CancellationToken GetCancelledCancellationToken()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            source.Cancel();
            return token;
        }
    }
}
