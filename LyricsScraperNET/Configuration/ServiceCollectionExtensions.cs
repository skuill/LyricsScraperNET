using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Genius;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
                services.AddAZLyricsClientService(lyricScraperClientConfig);
                services.AddGeniusClientService(lyricScraperClientConfig);

                services.Configure<LyricScraperClientConfig>(lyricScraperClientConfig);
                services.AddScoped<ILyricScraperClientConfig>(x => x.GetRequiredService<IOptionsSnapshot<LyricScraperClientConfig>>().Value);
            }

            services.AddScoped(typeof(ILyricsScraperClient), typeof(LyricsScraperClient));

            return services;
        }

        public static IServiceCollection AddAZLyricsClientService(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var configurationSection = configuration.GetSection(AZLyricsOptions.ConfigurationSectionName);
            if (configurationSection.Exists())
            {
                services.Configure<AZLyricsOptions>(configurationSection);

                services.AddScoped(typeof(IExternalProvider), typeof(AZLyricsProvider));
            }

            return services;
        }

        public static IServiceCollection AddGeniusClientService(
           this IServiceCollection services,
           IConfiguration configuration)
        {
            var configurationSection = configuration.GetSection(GeniusOptions.ConfigurationSectionName);
            if (configurationSection.Exists())
            {
                services.Configure<GeniusOptions>(configurationSection);

                services.AddScoped(typeof(IExternalProvider), typeof(GeniusProvider));
            }

            return services;
        }
    }
}
