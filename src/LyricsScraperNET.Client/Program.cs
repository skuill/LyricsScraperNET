using LyricsScraper;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Network.Html;
using LyricsScraperNET.Models;
using LyricsScraperNET.External.Abstract;
using LyricsScraperNET.External.AZLyrics;
using LyricsScraperNET.External.Genius;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

internal class Program
{
    private static async Task Main(string[] args)
    {
        IConfiguration Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();


        //// AZLyrics
        //var options = Options.Create(Configuration.GetRequiredSection("LyricScraperClientConfig:AZLyricsOptions")
        //    .Get<AZLyricsOptions>());
        //IExternalServiceClient<string> lyricClient = new AZLyricsClient(null, options);

        //// Genius
        var options = Options.Create(Configuration.GetRequiredSection("LyricScraperClientConfig:GeniusOptions")
            .Get<GeniusOptions>());
        IExternalServiceClient<string> lyricClient = new GeniusClient(null, options);

        SearchLyricDemo(lyricClient);

        Console.ReadLine();
    }

    public static void SearchLyricDemo(IExternalServiceClient<string> lyricClient)
    {
        ILyricsScraperClient<string> lyricsScraperClient = new LyricsScraperClient(null);
        lyricsScraperClient.AddClient(lyricClient);

        string artistToSearch = "Parkway Drive";
        string songToSearch = "Idols And Anchors";

        var searchRequest = new ArtistAndSongSearchRequest(artistToSearch, songToSearch);
        var result = lyricsScraperClient.SearchLyric(searchRequest);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"{artistToSearch} - {songToSearch}");
        Console.WriteLine();
        Console.ResetColor();
        Console.WriteLine(result);
    }
}
