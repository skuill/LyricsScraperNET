using LyricsScraperNET.Configuration;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.TestShared.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace LyricsScraperNET.UnitTest.Configuration
{
    public class ServiceCollectionExtensionsTest
    {
        private IConfiguration BuildConfigurationFromFile(string settingsPath = "")
        {
            var configuration = new ConfigurationBuilder();

            if (!string.IsNullOrEmpty(settingsPath))
                configuration.AddJsonFile(PathHelper.GetFullPathInCurrentDirectory(settingsPath), optional: false);

            return configuration.Build();
        }

        [Fact]
        public void IocContainer_GetService_LyricsScraperClient_DefaultNotNull()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var configuration = BuildConfigurationFromFile();
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            serviceCollection.AddLyricScraperClientService(configuration);

            var serviceProvider = serviceCollection.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                }).CreateScope().ServiceProvider;
            var service = serviceProvider.GetService<ILyricsScraperClient>();
            var searchResult = service.SearchLyric(new ArtistAndSongSearchRequest(null, null), cancellationToken);

            // Assert
            Assert.NotNull(service);
            Assert.False(service.IsEnabled);
            Assert.NotNull(searchResult);
            Assert.True(searchResult.IsEmpty());
            Assert.True(searchResult.ExternalProviderType == ExternalProviderType.None);
            Assert.True(string.IsNullOrEmpty(searchResult.LyricText));
        }

        [Fact]
        public void IocContainer_GetService_LyricsScraperClient_FullSetup()
        {
            // Arrange
            var externalProviderTypes = Enum.GetValues(typeof(ExternalProviderType))
                .Cast<ExternalProviderType>()
                .Where(p => p != ExternalProviderType.None);

            Dictionary<ExternalProviderType, int> expectedSearchPriorities = new Dictionary<ExternalProviderType, int>()
            {
                { ExternalProviderType.Genius, 11},
                { ExternalProviderType.AZLyrics, 22},
                { ExternalProviderType.Musixmatch, 33},
                { ExternalProviderType.SongLyrics, 44},
                { ExternalProviderType.LyricFind, 55}
            };

            string settingsPath = "Resources\\full_test_settings.json";

            var serviceCollection = new ServiceCollection();
            var configuration = BuildConfigurationFromFile(settingsPath);

            // Act
            serviceCollection.AddLyricScraperClientService(configuration);

            var serviceProvider = serviceCollection.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                }).CreateScope().ServiceProvider;
            var service = serviceProvider.GetService<ILyricsScraperClient>();

            // Assert
            Assert.NotNull(service);
            Assert.True(service.IsEnabled);

            foreach (var providerType in externalProviderTypes)
            {
                Assert.True(service[providerType].IsEnabled);
                Assert.Equal(expectedSearchPriorities[providerType], service[providerType].SearchPriority);
            }
        }
    }
}
