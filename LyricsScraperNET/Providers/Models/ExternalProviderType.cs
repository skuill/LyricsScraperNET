using System;

namespace LyricsScraperNET.Providers.Models
{
    /// <summary>
    /// External lyrics providers.
    /// Numeric values are part of the public contract (consumers may store or map them) and must not change.
    /// </summary>
    public enum ExternalProviderType
    {
        None = 0,
        AZLyrics = 1,
        Genius = 2,
        [Obsolete("The Musixmatch provider was removed in LyricsScraperNET 3.0.0. Searching with this provider type returns a BadRequest result.")]
        Musixmatch = 3,
        SongLyrics = 4,
        [Obsolete("The LyricFind provider was removed in LyricsScraperNET 3.0.0. Searching with this provider type returns a BadRequest result.")]
        LyricFind = 5,
        KPopLyrics = 6,
        LyricsFreak = 7,
        Lrclib = 8
    }
}
