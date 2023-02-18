using LyricsScraper;
using LyricsScraperNET.External.Abstract;
using LyricsScraperNET.External.AZLyrics;
using LyricsScraperNET.External.Genius;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LyricsScraperNET.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLyricScraperClientService<T>(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var lyricScraperClientConfig = configuration.GetSection(LyricScraperClientConfig.ConfigurationSectionName);
            if (lyricScraperClientConfig.Exists())
            {
                services.AddAZLyricsClientService<T>(lyricScraperClientConfig);
                services.AddGeniusClientService<T>(lyricScraperClientConfig);

                services.Configure<LyricScraperClientConfig>(lyricScraperClientConfig);
                services.AddScoped<ILyricScraperClientConfig>(x => x.GetRequiredService<IOptionsSnapshot<LyricScraperClientConfig>>().Value);
            }

            services.AddScoped(typeof(ILyricsScraperClient<T>), typeof(LyricsScraperClient));

            return services;
        }

        public static IServiceCollection AddAZLyricsClientService<T>(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var configurationSection = configuration.GetSection(AZLyricsOptions.ConfigurationSectionName);
            if (configurationSection.Exists())
            {
                services.Configure<AZLyricsOptions>(configurationSection);
                
                services.AddScoped(typeof(IExternalServiceClient<T>), typeof(AZLyricsClient));
            }

            return services;
        }

        public static IServiceCollection AddGeniusClientService<T>(
           this IServiceCollection services,
           IConfiguration configuration)
        {
            var configurationSection = configuration.GetSection(GeniusOptions.ConfigurationSectionName);
            if (configurationSection.Exists())
            {
                services.Configure<GeniusOptions>(configurationSection);

                services.AddScoped(typeof(IExternalServiceClient<T>), typeof(GeniusClient));                
            }

            return services;
        }
    }
}
