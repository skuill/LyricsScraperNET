using LyricsScraper;
using LyricsScraperNET.External.AZLyrics;
using LyricsScraperNET.External.Genius;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsScraperNET.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLyricScraperClientService(
            this IServiceCollection services,
            IConfiguration namedConfigurationSection)
        {
            // Default library options are overridden
            // by bound configuration values.
            services.Configure<LyricScraperClientConfig>(namedConfigurationSection);

            var AZLyricsConfigurationSection = namedConfigurationSection.GetRequiredSection("AZLyricsOptions");
            if (AZLyricsConfigurationSection != null) {
                services.Configure<AZLyricsOptions>(AZLyricsConfigurationSection);
            }

            var GeniusConfigurationSection = namedConfigurationSection.GetRequiredSection("GeniusOptions");
            if (GeniusConfigurationSection != null)
            {
                services.Configure<GeniusOptions>(GeniusConfigurationSection);
            }

            // Register lib services here...
            services.AddScoped<ILyricsScraperClient<string>, LyricsScraperClient>();

            return services;
        }
    }
}
