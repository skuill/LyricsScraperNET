using FakeItEasy;
using LyricsScraperNET.Configuration;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Network;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Lrclib;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.TestShared.Helpers;
using LyricsScraperNET.UnitTest.Network;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static LyricsScraperNET.UnitTest.Network.HttpTestHelper;

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
            Dictionary<ExternalProviderType, int> expectedSearchPriorities = new Dictionary<ExternalProviderType, int>()
            {
                { ExternalProviderType.Genius, 11},
                { ExternalProviderType.AZLyrics, 22},
                { ExternalProviderType.SongLyrics, 44},
                { ExternalProviderType.KPopLyrics, 66},
                { ExternalProviderType.LyricsFreak, 77},
                { ExternalProviderType.Lrclib, 88}
            };

            // The settings still contain sections of providers removed in 3.0.0. They must be ignored.
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
            Assert.True(service.UseParallelSearch);

            Assert.Equal(expectedSearchPriorities.Keys.OrderBy(x => x), ExternalProviderTypeHelper.SupportedProviderTypes.OrderBy(x => x));
            foreach (var providerType in ExternalProviderTypeHelper.SupportedProviderTypes)
            {
                Assert.True(service[providerType].IsEnabled);
                Assert.Equal(expectedSearchPriorities[providerType], service[providerType].SearchPriority);
            }

            foreach (var providerType in ExternalProviderTypeHelper.RemovedProviderTypes)
            {
                Assert.Null(service[providerType]);
            }
        }

        [Fact]
        public void IocContainer_ConfigWithRemovedProviders_LogsWarningOncePerRemovedSection()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["LyricScraperClient:GeniusOptions:Enabled"] = "true",
                    ["LyricScraperClient:MusixmatchOptions:Enabled"] = "true",
                    ["LyricScraperClient:MusixmatchOptions:SearchPriority"] = "33",
                    ["LyricScraperClient:LyricFindOptions:Enabled"] = "true",
                })
                .Build();

            var (loggerFactory, logger) = CreateFakeLoggerFactory();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(loggerFactory);

            var expectedMessages = new[]
            {
                "Configuration section 'LyricScraperClient:MusixmatchOptions' belongs to a provider that was removed in LyricsScraperNET 3.0.0. The section is ignored, please remove it from your configuration.",
                "Configuration section 'LyricScraperClient:LyricFindOptions' belongs to a provider that was removed in LyricsScraperNET 3.0.0. The section is ignored, please remove it from your configuration.",
            };

            // Act
            serviceCollection.AddLyricScraperClientService(configuration);

            var serviceProvider = serviceCollection.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });

            // Two scopes: the warning must not be repeated for every resolved client.
            var firstClient = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ILyricsScraperClient>();
            var secondClient = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ILyricsScraperClient>();

            // Assert
            Assert.True(firstClient.IsEnabled);
            Assert.True(secondClient.IsEnabled);
            Assert.NotNull(firstClient[ExternalProviderType.Genius]);
            foreach (var providerType in ExternalProviderTypeHelper.RemovedProviderTypes)
            {
                Assert.Null(firstClient[providerType]);
            }

            Assert.Equal(expectedMessages, GetLoggedMessages(logger, LogLevel.Warning));
            A.CallTo(logger)
                .Where(call => call.Method.Name == nameof(ILogger.Log))
                .MustHaveHappenedTwiceExactly();
        }

        [Theory]
        [InlineData("00:00:05", 5000)]
        [InlineData(null, 10000)]
        public void IocContainer_ProviderTimeoutFromConfig_IsUsedByClient(string configValue, int expectedMilliseconds)
        {
            // Arrange
            var settings = new Dictionary<string, string>
            {
                ["LyricScraperClient:GeniusOptions:Enabled"] = "true",
            };
            if (configValue != null)
                settings["LyricScraperClient:ProviderTimeout"] = configValue;

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            var serviceCollection = new ServiceCollection();

            // Act
            serviceCollection.AddLyricScraperClientService(configuration);

            var service = serviceCollection.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                }).CreateScope().ServiceProvider.GetRequiredService<ILyricsScraperClient>();

            // Assert
            Assert.Equal(TimeSpan.FromMilliseconds(expectedMilliseconds), service.ProviderTimeout);
            Assert.False(service.UseParallelSearch);
            Assert.True(service.IsEnabled);
        }

        [Fact]
        public void IocContainer_ConfigWithoutRemovedProviders_DoesNotLogWarning()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["LyricScraperClient:GeniusOptions:Enabled"] = "true",
                })
                .Build();

            var (loggerFactory, logger) = CreateFakeLoggerFactory();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(loggerFactory);

            // Act
            serviceCollection.AddLyricScraperClientService(configuration);

            var service = serviceCollection.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                }).CreateScope().ServiceProvider.GetRequiredService<ILyricsScraperClient>();

            // Assert
            Assert.True(service.IsEnabled);
            A.CallTo(logger)
                .Where(call => call.Method.Name == nameof(ILogger.Log))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task IocContainer_ConsumerConfiguresNamedHttpClient_ProvidersSendRequestsThroughIt()
        {
            // Arrange
            var sentRequests = new List<SentRequest>();
            var (_, handler) = CreateHttpClient(sentRequests, HttpStatusCode.NotFound, "<html>not found</html>");

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLyricScraperClientService(BuildConfigurationFromFile("Resources\\full_test_settings.json"));
            // What a consumer does to add a proxy, resilience handlers or an own User-Agent.
            serviceCollection.AddHttpClient(ServiceCollectionExtensions.HttpClientName)
                .ConfigureHttpClient(httpClient => httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ConsumerApp/2.0"))
                .ConfigurePrimaryHttpMessageHandler(() => handler);

            var service = serviceCollection.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                }).CreateScope().ServiceProvider.GetRequiredService<ILyricsScraperClient>();
            var expectedUri = new AZLyricsUriConverter().GetLyricUri("Artist", "Song");

            // Act
            var searchResult = await service[ExternalProviderType.AZLyrics].SearchLyricAsync(
                new ArtistAndSongSearchRequest("Artist", "Song"), TestContext.Current.CancellationToken);

            // Assert
            Assert.True(searchResult.IsEmpty());
            foreach (var providerType in ExternalProviderTypeHelper.SupportedProviderTypes)
            {
                var webClient = ((ExternalProviderBase)service[providerType]).WebClient;
                Assert.NotSame(SharedHttpClient.Instance, SharedHttpClientTests.GetHttpClient(webClient));
            }
            Assert.IsType<LrclibHttpClient>(((ExternalProviderBase)service[ExternalProviderType.Lrclib]).WebClient);
            Assert.Equal(new[] { new SentRequest(HttpMethod.Get, expectedUri, "ConsumerApp/2.0", null) }, Snapshot(sentRequests));
            CallToSendAsync(handler).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void IocContainer_NamedHttpClient_UsesSocketsHttpHandlerWithPooledConnectionLifetime()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLyricScraperClientService(BuildConfigurationFromFile("Resources\\full_test_settings.json"));
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            HttpMessageHandler handler = serviceProvider.GetRequiredService<IHttpMessageHandlerFactory>()
                .CreateHandler(ServiceCollectionExtensions.HttpClientName);
            while (handler is DelegatingHandler delegatingHandler)
                handler = delegatingHandler.InnerHandler!;

            // Assert
            var socketsHandler = Assert.IsType<SocketsHttpHandler>(handler);
            Assert.Equal(TimeSpan.FromMinutes(2), socketsHandler.PooledConnectionLifetime);
            Assert.Equal(DecompressionMethods.All, socketsHandler.AutomaticDecompression);
        }

        private static (ILoggerFactory LoggerFactory, ILogger Logger) CreateFakeLoggerFactory()
        {
            var logger = A.Fake<ILogger>();
            var loggerFactory = A.Fake<ILoggerFactory>();
            A.CallTo(() => loggerFactory.CreateLogger(A<string>._)).Returns(logger);
            return (loggerFactory, logger);
        }

        private static List<string> GetLoggedMessages(ILogger logger, LogLevel logLevel)
            => Fake.GetCalls(logger)
                .Where(call => call.Method.Name == nameof(ILogger.Log) && call.GetArgument<LogLevel>(0) == logLevel)
                .Select(call => call.Arguments[2].ToString())
                .ToList();
    }
}
