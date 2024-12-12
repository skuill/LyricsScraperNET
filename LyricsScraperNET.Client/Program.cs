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

        //// How to configure for a certain external provider using explicit instantiation:
        //var result = ExampleWithExplicitInstantiation(artistToSearch, songToSearch);

        //// Checking that something was found. The response can be empty in two cases:
        //// 1) A search error occurred. Detailed information can be found in the logs or in response fields like 'ResponseStatusCode' and 'ResponseMessage'.
        //// 2) The requested song contains only the instrumental, no lyrics. In this case the flag 'Instrumental' will be true.
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
                    $"Status code: [{result.ResponseStatusCode}]. " +
                    $"Response message: [{result.ResponseMessage}].").WriteLineColored(ConsoleColor.Red);
            }
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
    private static SearchResult ExampleWithExplicitInstantiation(string artistToSearch, string songToSearch)
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
        //     SearchPriority = 1 // If there are multiple external providers, then the search will start from the provider with the highest priority.
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
