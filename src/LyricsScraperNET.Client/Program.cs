using LyricsScraper;
using LyricsScraperNET.Configuration;
using LyricsScraperNET.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // Application Configuration section
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                // Setting up Lyric Scraper Client from configuration.
                // Only supported output type is string at the moment.
                services.AddLyricScraperClientService<string>(configuration: configuration);
            })
            .Build();

        // Application search example
        string artistToSearch = "Parkway Drive";
        string songToSearch = "Idols And Anchors";

        var lyricsScraperClient = host.Services.GetRequiredService<ILyricsScraperClient<string>>();
        
        var searchRequest = new ArtistAndSongSearchRequest(artistToSearch, songToSearch);
        var result = lyricsScraperClient.SearchLyric(searchRequest);

        // Output to console
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"{artistToSearch} - {songToSearch}");
        Console.WriteLine();
        Console.ResetColor();
        Console.WriteLine(result);
        Console.ReadLine();
    }
}
