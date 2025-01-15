using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Genius;
using LyricsScraperNET.Providers.LyricFind;
using LyricsScraperNET.Providers.Musixmatch;
using LyricsScraperNET.Providers.SongLyrics;
using LyricsScraperNET.Providers.KPopLyrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace LyricsScraperNET.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLyricScraperClientService(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var lyricScraperClientConfig = configuration.GetSection(LyricScraperClientConfig.ConfigurationSectionName);
            if (lyricScraperClientConfig.Exists())
            {
                services.AddProvider<AZLyricsOptions, AZLyricsProvider>(lyricScraperClientConfig);
                services.AddProvider<GeniusOptions, GeniusProvider>(lyricScraperClientConfig);
                services.AddProvider<SongLyricsOptions, SongLyricsProvider>(lyricScraperClientConfig);
                services.AddProvider<LyricFindOptions, LyricFindProvider>(lyricScraperClientConfig);
                services.AddProvider<KPopLyricsOptions, KPopLyricsProvider>(lyricScraperClientConfig);

                services.AddMusixmatchService(lyricScraperClientConfig);

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
            where TProvider : class, IExternalProvider
        {
            var configurationSection = configuration.GetSection(Activator.CreateInstance<TOptions>().ConfigurationSectionName);
            if (configurationSection.Exists())
            {
                services.Configure<TOptions>(configurationSection);
                services.AddScoped<IExternalProvider, TProvider>();
            }

            return services;
        }

        private static IServiceCollection AddMusixmatchService(
           this IServiceCollection services,
           IConfiguration configuration)
        {
            var configurationSection = configuration.GetSection(Activator.CreateInstance<MusixmatchOptions>().ConfigurationSectionName);
            if (configurationSection.Exists())
            {
                services.Configure<MusixmatchOptions>(configurationSection);

                services.AddSingleton<IMusixmatchTokenCache, MusixmatchTokenCache>();
                services.AddScoped<IMusixmatchClientWrapper, MusixmatchClientWrapper>();
                services.AddScoped(typeof(IExternalProvider), typeof(MusixmatchProvider));
            }

            return services;
        }
    }
}
