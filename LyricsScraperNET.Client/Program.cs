using LyricsScraperNET.Configuration;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Genius;
using LyricsScraperNET.Providers.KPopLyrics;
using LyricsScraperNET.Providers.Lrclib;
using LyricsScraperNET.Providers.LyricsFreak;
using LyricsScraperNET.Providers.SongLyrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace LyricsScraperNET.Client
{
    class Program
    {
        static async Task Main()
        {
            //// Input parameters to search:
            string artistToSearch = "Parkway Drive";
            string songToSearch = "Idols And Anchors";

            //// Uncomment this block to test for instrumental songs (without vocals):
            //string artistToSearch = "Rush";
            //string songToSearch = "YYZ";

            //// How to configure for ASP.NET applications:
            var result = await ExampleWithHostConfiguration(artistToSearch, songToSearch);

            //// How to configure for a certain external provider using explicit instantiation:
            //var result = await ExampleWithExplicitInstantiation(artistToSearch, songToSearch);

            //// How to configure from appsettings.json without dependency injection:
            //var result = await ExampleWithConfigurationWithoutDependencyInjection(artistToSearch, songToSearch);

            //// Checking that something was found: 'Code' tells why the search ended, for example:
            //// - Found: the lyrics are in 'LyricText';
            //// - Instrumental: the song has no lyrics, 'LyricText' is empty;
            //// - NotFound: at least one provider answered, none has the lyrics;
            //// - AllProvidersFailed: no provider could check the request (site unavailable, timeout or error), the lyrics may exist.
            //// 'ProviderAttempts' shows what each provider did, 'ResponseMessage' is a text for humans.
            if (result.IsEmpty())
            {
                ConsoleExtensions.WriteLineDelimeter();
                if (result.Instrumental)
                {
                    $"This song [{artistToSearch} - {songToSearch}] is instrumental.\r\nIt does not contain any lyrics"
                        .WriteLineColored(ConsoleColor.Gray);
                }
                else
                {
                    ($"Can't find lyrics for: [{artistToSearch} - {songToSearch}]. " +
                        $"Code: [{result.Code}]. " +
                        $"Response message: [{result.ResponseMessage}].").WriteLineColored(ConsoleColor.Red);
                }

                foreach (var attempt in result.ProviderAttempts)
                    attempt.ToString().WriteLineColored(ConsoleColor.DarkGray);
                ConsoleExtensions.WriteLineDelimeter();

                "Press any key to exit..".WriteLineColored(ConsoleColor.DarkGray);
                Console.ReadLine();
                return;
            }

            //// Output result to console
            //// Artist and song information
            $"[{artistToSearch} - {songToSearch}]".WriteLineColored(ConsoleColor.Yellow);

            ConsoleExtensions.WriteLineDelimeter();
            //// Lyric text
            result.LyricText.WriteLineColored();
            ConsoleExtensions.WriteLineDelimeter();

            //// Lyrics provider information
            $"This lyric was found by [{result.ExternalProviderType}]\r\n".WriteLineColored(ConsoleColor.Magenta);

            //// What each provider did, for example "Lrclib: NotFound (180 ms)" or "AZLyrics: Failed (95 ms): HTTP 403"
            foreach (var attempt in result.ProviderAttempts)
                attempt.ToString().WriteLineColored(ConsoleColor.DarkGray);

            "Press any key to exit..".WriteLineColored(ConsoleColor.DarkGray);
            Console.ReadLine();
            return;
        }

        /// <summary>
        /// How to configure LyricScraperClient and search lyrics for ASP.NET applications:
        /// </summary>
        /// <param name="artistToSearch">artist name to search</param>
        /// <param name="songToSearch">song name to search</param>
        /// <returns>lyrics text</returns>
        private static async Task<SearchResult> ExampleWithHostConfiguration(string artistToSearch, string songToSearch)
        {
            //// Application Configuration section. 
            //// LyricScraperClient configuration could be found in appsettings.json file in section with related name.
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            //// Host creation with LyricScraperClient service configuration
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    // Setting up LyricScraperClient from configuration that stored in appsettings.json.
                    // Only supported output type as string at the moment.
                    services.AddLyricScraperClientService(configuration: configuration);
                })
                .Build();

            //// Get instance of LyricScraperClient service 
            var lyricsScraperClient = host.Services.GetRequiredService<ILyricsScraperClient>();

            //// Create request and search 
            var searchRequest = new ArtistAndSongSearchRequest(artistToSearch, songToSearch);
            CancellationToken cancellationToken = new CancellationToken();
            var result = await lyricsScraperClient.SearchLyricAsync(searchRequest, cancellationToken);

            return result;
        }

        /// <summary>
        /// How to configure LyricScraperClient from appsettings.json without dependency injection:
        /// the same sections as for the host configuration, the providers are created by hand.
        /// </summary>
        /// <param name="artistToSearch">artist name to search</param>
        /// <param name="songToSearch">song name to search</param>
        /// <returns>lyrics text</returns>
        private static async Task<SearchResult> ExampleWithConfigurationWithoutDependencyInjection(string artistToSearch, string songToSearch)
        {
            //// Read the "LyricScraperClient" section: client settings and one section per provider (Enabled, SearchPriority, ApiKey for Genius).
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            var clientConfig = configuration
                .GetSection(LyricScraperClientConfig.ConfigurationSectionName)
                .Get<LyricScraperClientConfig>() ?? new LyricScraperClientConfig();

            //// Create the providers with their options from the configuration.
            IExternalProvider[] providers =
            {
                new GeniusProvider((GeniusOptions)clientConfig.GeniusOptions),
                new AZLyricsProvider((AZLyricsOptions)clientConfig.AZLyricsOptions),
                new SongLyricsProvider((SongLyricsOptions)clientConfig.SongLyricsOptions),
                new KPopLyricsProvider((KPopLyricsOptions)clientConfig.KPopLyricsOptions),
                new LyricsFreakProvider((LyricsFreakOptions)clientConfig.LyricsFreakOptions),
                new LrclibProvider((LrclibOptions)clientConfig.LrclibOptions),
            };

            //// The client takes UseParallelSearch and ProviderTimeout from the configuration; disabled providers are skipped.
            ILyricsScraperClient lyricsScraperClient = new LyricsScraperClient(clientConfig, providers);

            var searchRequest = new ArtistAndSongSearchRequest(artistToSearch, songToSearch);
            return await lyricsScraperClient.SearchLyricAsync(searchRequest, CancellationToken.None);
        }

        /// <summary>
        /// How to configure LyricScraperClient and search lyrics for a certain external provider:
        /// </summary>
        /// <param name="artistToSearch">artist name to search</param>
        /// <param name="songToSearch">song name to search</param>
        /// <returns>lyrics text</returns>
        private static async Task<SearchResult> ExampleWithExplicitInstantiation(string artistToSearch, string songToSearch)
        {
            //// Create instance of LyricScraperClient with all available lyrics providers
            ILyricsScraperClient lyricsScraperClient
                = new LyricsScraperClient()
                    .WithAllProviders();

            //// To configure a specific provider, use a method like With[ProviderName]()
            // ILyricsScraperClient lyricsScraperClient
            //     = new LyricsScraperClient()
            //         .WithGenius()
            //         .WithAZLyrics()
            //         .WithSongLyrics()
            //         .WithLrclib();

            //// To perform parallel searches across multiple external providers, enable the following option:
            // lyricsScraperClient.UseParallelSearch = true;

            //// Maximum time one provider gets to answer (default: 10 seconds, Timeout.InfiniteTimeSpan disables it):
            // lyricsScraperClient.ProviderTimeout = TimeSpan.FromSeconds(5);

            // To enable library logging, the LoggerFactory must be configured and passed to the client.
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddFilter("Microsoft", LogLevel.Warning)
                       .AddFilter("System", LogLevel.Warning)
                       .AddFilter("LyricsScraperNET", LogLevel.Trace)
                       .AddConsole();
            });
            lyricsScraperClient.WithLogger(loggerFactory);

            //// Another way to configure:
            //// 1. First create instance of LyricScraperClient.
            // ILyricsScraperClient lyricsScraperClient = new LyricsScraperClient();
            //// 2. Create some external provider instanse with default settings. For example Genius:
            // IExternalProvider externalProvider = new GeniusProvider();
            //// 2. Or create provider with custom settings like:
            // GeniusOptions geniusOptions = new GeniusOptions()
            // {
            //     Enabled = true,
            //     SearchPriority = 1 // If there are multiple external providers, then the search will start from the provider with the highest priority.
            // };
            // IExternalProvider externalProvider = new GeniusProvider(geniusOptions);
            //// 3. Add external provider to client:
            // lyricsScraperClient.AddProvider(externalProvider);

            //// Create request and search 
            var searchRequest = new ArtistAndSongSearchRequest(artistToSearch, songToSearch);
            CancellationToken cancellationToken = new CancellationToken();
            var result = await lyricsScraperClient.SearchLyricAsync(searchRequest, cancellationToken);

            return result;
        }
    }
}

