namespace LyricsScraperNET.Models.Requests
{
    public sealed class ArtistAndSongSearchRequest : SearchRequest
    {
        public string Artist { get; }

        public string Song { get; }

        public ArtistAndSongSearchRequest(string artist, string song)
        {
            Artist = artist;
            Song = song;
        }
    }
}
