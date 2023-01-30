namespace LyricsScraperNET.Models
{
    public record ArtistAndSongSearchRequest(string Artist, string Song) : SearchRequest;
}
