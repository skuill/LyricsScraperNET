using LyricsScraper.Abstract;
using LyricsScraper;
using LyricsScraper.AZLyrics;
using LyricsScraper.Genius;
using LyricsScraper.Network.Abstract;
using LyricsScraper.Network.Html;


//// AZLyrics
//ILyricWebClient lyricWebClient = new HtmlAgilityWebClient();
//ILyricParser lyricParser = new AZLyricsParser();
//ILyricClient lyricClient = new AZLyricsClient(null, lyricParser, lyricWebClient);

//ILyricsScraperUtil lyricsScraperUtil = new LyricsScraperUtil(null);
//lyricsScraperUtil.AddClient(lyricClient);

//string artistToSearch = "Parkway Drive";
//string songToSearch = "Idols And Anchors";

//var result = lyricsScraperUtil.SearchLyric(artistToSearch, songToSearch);

//Console.ForegroundColor = ConsoleColor.Yellow;
//Console.WriteLine($"{artistToSearch} - {songToSearch}");
//Console.WriteLine();
//Console.ResetColor();
//Console.WriteLine(result);

//Console.ReadLine();

// Genius
ILyricWebClient lyricWebClient = new HtmlAgilityWebClient();
ILyricParser<string> lyricParser = new GeniusParser();
ILyricClient<string> lyricClient = new GeniusClient(null, lyricParser, lyricWebClient, "mz9Cdxgu_wGqeiRGPH_FbO3b2g60EaBath_yO4jD2NC_SG4uDB8_gxyF9faILc6A");

ILyricsScraperClient<string> lyricsScraperUtil = new LyricsScraperClient(null);
lyricsScraperUtil.AddClient(lyricClient);

string artistToSearch = "Parkway Drive";
string songToSearch = "Idols And Anchors";

var result = lyricsScraperUtil.SearchLyric(artistToSearch, songToSearch);

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine($"{artistToSearch} - {songToSearch}");
Console.WriteLine();
Console.ResetColor();
Console.WriteLine(result);

Console.ReadLine();
