using LyricsScraperNET;
using LyricsScraperNET.Configuration;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Genius;
using LyricsScraperNET.Models.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal class Program
{
    private static async Task Main(string[] args)
    {
        //// Input parameters to search:
        string artistToSearch = "Parkway Drive";
        string songToSearch = "Idols And Anchors";

        //// How to configure for ASP.NET applications:
        //var result = ExampleWithHostConfiguration(artistToSearch, songToSearch);

        //// How to configure for a certain external provider:
        var result = ExampleWithCertainProvider(artistToSearch, songToSearch);

        //// Output to console
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"{artistToSearch} - {songToSearch}");
        Console.WriteLine();
        Console.ResetColor();
        Console.WriteLine(result);
        Console.ReadLine();
    }

    /// <summary>
    /// How to configure LyricScraperClient and search lyrics for ASP.NET applications:
    /// </summary>
    /// <param name="artistToSearch">artist name to search</param>
    /// <param name="songToSearch">song name to search</param>
    /// <returns>lyrics text</returns>
    private static string ExampleWithHostConfiguration(string artistToSearch, string songToSearch)
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
                services.AddLyricScraperClientService<string>(configuration: configuration);
            })
            .Build();

        //// Get instance of LyricScraperClient service 
        var lyricsScraperClient = host.Services.GetRequiredService<ILyricsScraperClient<string>>();

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
    private static string ExampleWithCertainProvider(string artistToSearch, string songToSearch)
    {
        //// Create instance of LyricScraperClient with Genius and AZLyrics providers
        ILyricsScraperClient<string> lyricsScraperClient 
            = new LyricsScraperClient()
                .WithGenius()
                .WithAZLyrics();

        //// Another way to configure:
        //// 1. First create instance of LyricScraperClient.
        // ILyricsScraperClient<string> lyricsScraperClient = new LyricsScraperClient();
        //// 2. Create some external provider instanse. For example Genius:
        // IExternalProvider<string> externalProvider = new GeniusProvider();
        //// 3. Add external provider to client:
        // lyricsScraperClient.AddProvider(externalProvider);

        //// Create request and search 
        var searchRequest = new ArtistAndSongSearchRequest(artistToSearch, songToSearch);
        var result = lyricsScraperClient.SearchLyric(searchRequest);

        return result;
    }
}
