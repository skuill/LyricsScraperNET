using LyricsScraperNET.Common;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.TestShared.Helpers;
using Xunit;

namespace LyricsScraperNET.UnitTest.Extensions
{
    public class LyricsScraperClientExtensionsTest
    {
        private ILyricsScraperClient _lyricsScraperClient => new LyricsScraperClient();

        [Fact]
        public void LyricsScraperClient_WithLyricsFreak_ReturnsIsEnabled()
        {
            // Act
            var lyricsScraperClient = _lyricsScraperClient.WithLyricsFreak();
            var externalTypeProvider = lyricsScraperClient[ExternalProviderType.LyricsFreak];

            // Assert
            Assert.NotNull(lyricsScraperClient);
            Assert.True(lyricsScraperClient.IsEnabled);
            Assert.NotNull(externalTypeProvider);
            Assert.True(externalTypeProvider.IsEnabled);
            Assert.Equal(Constants.ProvidersSearchPriorities[ExternalProviderType.LyricsFreak], externalTypeProvider.SearchPriority);
        }

        [Fact]
        public void LyricsScraperClient_WithAZLyrics_ReturnsIsEnabled()
        {
            // Act
            var lyricsScraperClient = _lyricsScraperClient.WithAZLyrics();
            var externalTypeProvider = lyricsScraperClient[ExternalProviderType.AZLyrics];

            // Assert
            Assert.NotNull(lyricsScraperClient);
            Assert.True(lyricsScraperClient.IsEnabled);
            Assert.NotNull(externalTypeProvider);
            Assert.True(externalTypeProvider.IsEnabled);
            Assert.Equal(Constants.ProvidersSearchPriorities[ExternalProviderType.AZLyrics], externalTypeProvider.SearchPriority);
        }

        [Fact]
        public void LyricsScraperClient_WithGenius_ReturnsIsEnabled()
        {
            // Act
            var lyricsScraperClient = _lyricsScraperClient.WithGenius();
            var externalTypeProvider = lyricsScraperClient[ExternalProviderType.Genius];

            // Assert
            Assert.NotNull(lyricsScraperClient);
            Assert.True(lyricsScraperClient.IsEnabled);
            Assert.NotNull(externalTypeProvider);
            Assert.True(externalTypeProvider.IsEnabled);
            Assert.Equal(Constants.ProvidersSearchPriorities[ExternalProviderType.Genius], externalTypeProvider.SearchPriority);
        }

        [Fact]
        public void LyricsScraperClient_WithMusixmatch_RemovedProvider_AddsNothing()
        {
            // Arrange
            var client = _lyricsScraperClient;

            // Act
#pragma warning disable LSNET0003 // Obsolete: Musixmatch was removed in 3.0.0, WithMusixmatch() in 4.0.0
            var lyricsScraperClient = client.WithMusixmatch();
            var externalTypeProvider = lyricsScraperClient[ExternalProviderType.Musixmatch];
#pragma warning restore LSNET0003

            // Assert
            Assert.Same(client, lyricsScraperClient);
            Assert.False(lyricsScraperClient.IsEnabled);
            Assert.Null(externalTypeProvider);
        }

        [Fact]
        public void LyricsScraperClient_WithKPopLyrics_ReturnsIsEnabled()
        {
            // Act
            var lyricsScraperClient = _lyricsScraperClient.WithKPopLyrics();
            var externalTypeProvider = lyricsScraperClient[ExternalProviderType.KPopLyrics];

            // Assert
            Assert.NotNull(lyricsScraperClient);
            Assert.True(lyricsScraperClient.IsEnabled);
            Assert.NotNull(externalTypeProvider);
            Assert.True(externalTypeProvider.IsEnabled);
            Assert.Equal(Constants.ProvidersSearchPriorities[ExternalProviderType.KPopLyrics], externalTypeProvider.SearchPriority);
        }

        [Fact]
        public void LyricsScraperClient_WithSongLyrics_ReturnsIsEnabled()
        {
            // Act
            var lyricsScraperClient = _lyricsScraperClient.WithSongLyrics();
            var externalTypeProvider = lyricsScraperClient[ExternalProviderType.SongLyrics];

            // Assert
            Assert.NotNull(lyricsScraperClient);
            Assert.True(lyricsScraperClient.IsEnabled);
            Assert.NotNull(externalTypeProvider);
            Assert.True(externalTypeProvider.IsEnabled);
            Assert.Equal(Constants.ProvidersSearchPriorities[ExternalProviderType.SongLyrics], externalTypeProvider.SearchPriority);
        }

        [Fact]
        public void LyricsScraperClient_WithLyricFind_RemovedProvider_AddsNothing()
        {
            // Arrange
            var client = _lyricsScraperClient;

            // Act
#pragma warning disable LSNET0004 // Obsolete: LyricFind was removed in 3.0.0, WithLyricFind() in 4.0.0
            var lyricsScraperClient = client.WithLyricFind();
            var externalTypeProvider = lyricsScraperClient[ExternalProviderType.LyricFind];
#pragma warning restore LSNET0004

            // Assert
            Assert.Same(client, lyricsScraperClient);
            Assert.False(lyricsScraperClient.IsEnabled);
            Assert.Null(externalTypeProvider);
        }

        [Fact]
        public void LyricsScraperClient_WithLrclib_ReturnsIsEnabled()
        {
            // Act
            var lyricsScraperClient = _lyricsScraperClient.WithLrclib();
            var externalTypeProvider = lyricsScraperClient[ExternalProviderType.Lrclib];

            // Assert
            Assert.NotNull(lyricsScraperClient);
            Assert.True(lyricsScraperClient.IsEnabled);
            Assert.NotNull(externalTypeProvider);
            Assert.True(externalTypeProvider.IsEnabled);
            Assert.Equal(Constants.ProvidersSearchPriorities[ExternalProviderType.Lrclib], externalTypeProvider.SearchPriority);
        }

        [Fact]
        public void LyricsScraperClient_WithAllProviders_ReturnsIsEnabled()
        {
            // Act
            var lyricsScraperClient = _lyricsScraperClient.WithAllProviders();

            Assert.NotNull(lyricsScraperClient);
            Assert.True(lyricsScraperClient.IsEnabled);

            foreach (var providerType in ExternalProviderTypeHelper.SupportedProviderTypes)
            {
                var externalTypeProvider = lyricsScraperClient[providerType];

                // Assert
                Assert.NotNull(externalTypeProvider);
                Assert.True(externalTypeProvider.IsEnabled);
                Assert.Equal(Constants.ProvidersSearchPriorities[providerType], externalTypeProvider.SearchPriority);
            }

            foreach (var providerType in ExternalProviderTypeHelper.RemovedProviderTypes)
            {
                Assert.Null(lyricsScraperClient[providerType]);
            }
        }
    }
}
