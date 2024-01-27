using LyricsScraperNET.Providers.Models;

namespace LyricsScraperNET.Models.Requests
{
    /// <summary>
    /// Query model for searching lyrics by artist/band name and song/track title.
    /// </summary>
    public sealed class ArtistAndSongSearchRequest : SearchRequest
    {
        /// <summary>
        /// Artist or band name.
        /// </summary>
        public string Artist { get; }

        /// <summary>
        /// Song or track title.
        /// </summary>
        public string Song { get; }

        /// <summary>
        /// The type of external provider for which lyrics will be searched.
        /// By default, it is set to <see cref="ExternalProviderType.None"/> - the search will be performed across all available client providers.
        /// </summary>
        public ExternalProviderType Provider { get; } = ExternalProviderType.None;

        public ArtistAndSongSearchRequest(string artist, string song)
        {
            Artist = artist;
            Song = song;
        }

        public ArtistAndSongSearchRequest(string artist, string song, ExternalProviderType provider)
            : this(artist, song)
        {
            Provider = provider;
        }

        public override string ToString()
        {
            return $"Artist: [{Artist}]. Song: [{Song}]. Provider: [{Provider}]";
        }
    }
}
