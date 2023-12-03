using LyricsScraperNET.Configuration;
using LyricsScraperNET.TestShared.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            // Act
            serviceCollection.AddLyricScraperClientService(configuration);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ILyricsScraperClient>();

            // Assert
            Assert.NotNull(service);
            Assert.False(service.IsEnabled);
        }

        [Fact]
        public void IocContainer_GetService_LyricsScraperClient_FullSetup()
        {
            // Arrange
            string settingsPath = "Resources\\full_test_settings.json";

            var serviceCollection = new ServiceCollection();
            var configuration = BuildConfigurationFromFile(settingsPath);

            // Act
            serviceCollection.AddLyricScraperClientService(configuration);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<ILyricsScraperClient>();

            // Assert
            Assert.NotNull(service);
            Assert.True(service.IsEnabled);
        }
    }
}
