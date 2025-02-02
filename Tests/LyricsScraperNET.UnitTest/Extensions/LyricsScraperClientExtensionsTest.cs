﻿using LyricsScraperNET.Providers.Models;
using System;
using System.Linq;
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
            Assert.Equal(6, externalTypeProvider.SearchPriority);
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
            Assert.Equal(5, externalTypeProvider.SearchPriority);
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
            Assert.Equal(4, externalTypeProvider.SearchPriority);
        }

        [Fact]
        public void LyricsScraperClient_WithMusixmatch_ReturnsIsEnabled()
        {
            // Act
            var lyricsScraperClient = _lyricsScraperClient.WithMusixmatch();
            var externalTypeProvider = lyricsScraperClient[ExternalProviderType.Musixmatch];

            // Assert
            Assert.NotNull(lyricsScraperClient);
            Assert.True(lyricsScraperClient.IsEnabled);
            Assert.NotNull(externalTypeProvider);
            Assert.True(externalTypeProvider.IsEnabled);
            Assert.Equal(0, externalTypeProvider.SearchPriority);
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
            Assert.Equal(3, externalTypeProvider.SearchPriority);
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
            Assert.Equal(2, externalTypeProvider.SearchPriority);
        }

        [Fact]
        public void LyricsScraperClient_WithLyricFind_ReturnsIsEnabled()
        {
            // Act
            var lyricsScraperClient = _lyricsScraperClient.WithLyricFind();
            var externalTypeProvider = lyricsScraperClient[ExternalProviderType.LyricFind];

            // Assert
            Assert.NotNull(lyricsScraperClient);
            Assert.True(lyricsScraperClient.IsEnabled);
            Assert.NotNull(externalTypeProvider);
            Assert.True(externalTypeProvider.IsEnabled);
            Assert.Equal(1, externalTypeProvider.SearchPriority);
        }

        [Fact]
        public void LyricsScraperClient_WithAllProviders_ReturnsIsEnabled()
        {
            // Act
            var lyricsScraperClient = _lyricsScraperClient.WithAllProviders();

            Assert.NotNull(lyricsScraperClient);
            Assert.True(lyricsScraperClient.IsEnabled);

            foreach (var providerType in Enum.GetValues(typeof(ExternalProviderType)).Cast<ExternalProviderType>())
            {
                if (providerType == ExternalProviderType.None)
                    continue;
                var externalTypeProvider = lyricsScraperClient[providerType];

                // Assert
                Assert.NotNull(externalTypeProvider);
                Assert.True(externalTypeProvider.IsEnabled);
            }
        }
    }
}
