using LyricsScraperNET;
using LyricsScraperNET.Configuration;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Genius;
using LyricsScraperNET.Providers.SongLyrics;
using LyricsScraperNET.Providers.LyricFind;
using LyricsScraperNET.Models.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LyricsScraperNET.Models.Responses;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main()
    {
        //// Input parameters to search:
        string artistToSearch = "Parkway Drive";
        string songToSearch = "Idols And Anchors";

        //// The case when a song contains only an instrumental, without vocals.
        //string artistToSearch = "Rush";
        //string songToSearch = "YYZ";

        //// How to configure for ASP.NET applications:
        var result = ExampleWithHostConfiguration(artistToSearch, songToSearch);

        //// How to configure for a certain external provider:
        //var result = ExampleWithCertainProvider(artistToSearch, songToSearch);

        //// Checking that something was found. The response can be empty in two cases:
        //// 1) A search error occurred. Detailed information can be found in the logs or in response fields like 'ResponseStatusCode' and 'ResponseMessage'.
        //// 2) The requested song contains only the instrumental, no lyrics. In this case the flag 'Instrumental' will be true.
        if (result.IsEmpty())
        {
            if (result.Instrumental)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"This song [{artistToSearch} - {songToSearch}] is instrumental.\r\nIt does not contain any lyrics");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Can't find lyrics for: [{artistToSearch} - {songToSearch}]. " +
                    $"Status code: [{result.ResponseStatusCode}]. " +
                    $"Response message: [{result.ResponseMessage}].");
            }
            Console.ResetColor();

            Console.ReadLine();
            return;
        }

        //// Output result to console
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[{artistToSearch} - {songToSearch}]\r\n");
        Console.ResetColor();

        Console.WriteLine(result.LyricText);

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"\r\nThis lyric was found by [{result.ExternalProviderType}]\r\n");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("Press any key to exit..");
        Console.ResetColor();
        Console.ReadLine();
    }

    /// <summary>
    /// How to configure LyricScraperClient and search lyrics for ASP.NET applications:
    /// </summary>
    /// <param name="artistToSearch">artist name to search</param>
    /// <param name="songToSearch">song name to search</param>
    /// <returns>lyrics text</returns>
    private static SearchResult ExampleWithHostConfiguration(string artistToSearch, string songToSearch)
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
        var result = lyricsScraperClient.SearchLyric(searchRequest);

        return result;
    }

    /// <summary>
    /// How to configure LyricScraperClient and search lyrics for a certain external provider:
    /// </summary>
    /// <param name="artistToSearch">artist name to search</param>
    /// <param name="songToSearch">song name to search</param>
    /// <returns>lyrics text</returns>
    private static SearchResult ExampleWithCertainProvider(string artistToSearch, string songToSearch)
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
        //         .WithMusixmatch()
        //         .WithSongLyrics()
        //         .WithLyricFind();

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
        //     SearchPriority = 1
        // };
        // IExternalProvider externalProvider = new GeniusProvider(geniusOptions);
        //// 3. Add external provider to client:
        // lyricsScraperClient.AddProvider(externalProvider);

        //// Create request and search 
        var searchRequest = new ArtistAndSongSearchRequest(artistToSearch, songToSearch);
        var result = lyricsScraperClient.SearchLyric(searchRequest);

        return result;
    }
}
