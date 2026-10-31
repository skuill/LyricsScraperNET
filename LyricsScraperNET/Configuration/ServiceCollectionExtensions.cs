using LyricsScraperNET.Network;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Genius;
using LyricsScraperNET.Providers.KPopLyrics;
using LyricsScraperNET.Providers.Lrclib;
using LyricsScraperNET.Providers.LyricsFreak;
using LyricsScraperNET.Providers.SongLyrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;

namespace LyricsScraperNET.Configuration
{
    /// <summary>
    /// Extension methods for configuring the LyricsScraperNET services in the DI container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Name of the <see cref="HttpClient"/> that the providers get from <see cref="IHttpClientFactory"/>.
        /// Configure it with <c>services.AddHttpClient(ServiceCollectionExtensions.HttpClientName)</c>,
        /// for example to add a proxy, resilience handlers or an own User-Agent.
        /// </summary>
        public const string HttpClientName = "LyricsScraperNET";

        /// <summary>
        /// Adds the LyricsScraperNET services to the service collection with automatic provider discovery.
        /// The providers send their requests through the <see cref="HttpClientName"/> client of <see cref="IHttpClientFactory"/>.
        /// </summary>
        public static IServiceCollection AddLyricScraperClientService(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var lyricScraperClientConfig = configuration.GetSection(LyricScraperClientConfig.ConfigurationSectionName);
            if (lyricScraperClientConfig.Exists())
            {
                // SocketsHttpHandler with PooledConnectionLifetime keeps DNS fresh even if a consumer holds a client for long:
                // https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory#using-ihttpclientfactory-together-with-socketshttphandler
                services.AddHttpClient(HttpClientName)
                    .UseSocketsHttpHandler((handler, _) => SharedHttpClient.ConfigureHandler(handler));

                services.AddProvider<LyricsFreakOptions, LyricsFreakProvider>(lyricScraperClientConfig);
                services.AddProvider<AZLyricsOptions, AZLyricsProvider>(lyricScraperClientConfig);
                services.AddProvider<GeniusOptions, GeniusProvider>(lyricScraperClientConfig);
                services.AddProvider<SongLyricsOptions, SongLyricsProvider>(lyricScraperClientConfig);
                services.AddProvider<KPopLyricsOptions, KPopLyricsProvider>(lyricScraperClientConfig);
                services.AddProvider<LrclibOptions, LrclibProvider>(lyricScraperClientConfig);

                services.AddRemovedProvidersWarning(lyricScraperClientConfig);

                services.Configure<LyricScraperClientConfig>(lyricScraperClientConfig);
                services.AddScoped<ILyricScraperClientConfig>(x => x.GetRequiredService<IOptionsSnapshot<LyricScraperClientConfig>>().Value);
            }

            services.AddScoped(typeof(ILyricsScraperClient), typeof(LyricsScraperClient));

            return services;
        }

        private static IServiceCollection AddProvider<TOptions, TProvider>(
            this IServiceCollection services,
            IConfiguration configuration)
            where TOptions : class, IExternalProviderOptions
            where TProvider : ExternalProviderBase
        {
            var configurationSection = configuration.GetSection(Activator.CreateInstance<TOptions>().ConfigurationSectionName);
            if (configurationSection.Exists())
            {
                services.Configure<TOptions>(configurationSection);
                services.AddScoped<TProvider>();
                // Clients from IHttpClientFactory are meant to be short-lived, so each scoped provider gets a new one.
                services.AddScoped<IExternalProvider>(serviceProvider =>
                {
                    var provider = serviceProvider.GetRequiredService<TProvider>();
                    provider.WithHttpClient(serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName));
                    return provider;
                });
            }

            return services;
        }

        /// <summary>
        /// Sections of removed providers are ignored. Registers a one-time warning if the configuration still contains them.
        /// </summary>
        private static IServiceCollection AddRemovedProvidersWarning(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var removedSectionNames = RemovedProvidersWarning.RemovedProviderSectionNames
                .Where(sectionName => configuration.GetSection(sectionName).Exists())
                .ToArray();

            if (removedSectionNames.Length > 0)
            {
                services.AddSingleton<IPostConfigureOptions<LyricScraperClientConfig>>(serviceProvider =>
                    new RemovedProvidersWarning(serviceProvider.GetService<ILoggerFactory>(), removedSectionNames));
            }

            return services;
        }
    }
}
