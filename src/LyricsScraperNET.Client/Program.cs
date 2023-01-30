using LyricsScraperNET.Abstract;
using LyricsScraper;
using LyricsScraperNET.AZLyrics;
using LyricsScraperNET.Genius;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Network.Html;
using LyricsScraperNET.Models;

internal class Program
{
    private static async Task Main(string[] args)
    {
        //// AZLyrics
        IExternalServiceClient<string> lyricClient = new AZLyricsClient(null);

        //// Genius
        //IExternalServiceClient<string> lyricClient = new GeniusClient(null, "mz9Cdxgu_wGqeiRGPH_FbO3b2g60EaBath_yO4jD2NC_SG4uDB8_gxyF9faILc6A");

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
