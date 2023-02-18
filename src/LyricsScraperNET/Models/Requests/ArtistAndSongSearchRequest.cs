namespace LyricsScraperNET.Models.Requests
{
    public record ArtistAndSongSearchRequest(string Artist, string Song) : SearchRequest;
}
