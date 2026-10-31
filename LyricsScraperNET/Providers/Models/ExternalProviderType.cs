using LyricsScraperNET.Common;
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
        [Obsolete(Obsoletions.MusixmatchRemoved.ProviderTypeMessage, DiagnosticId = Obsoletions.MusixmatchRemoved.DiagnosticId, UrlFormat = Obsoletions.SharedUrlFormat)]
        Musixmatch = 3,
        SongLyrics = 4,
        [Obsolete(Obsoletions.LyricFindRemoved.ProviderTypeMessage, DiagnosticId = Obsoletions.LyricFindRemoved.DiagnosticId, UrlFormat = Obsoletions.SharedUrlFormat)]
        LyricFind = 5,
        KPopLyrics = 6,
        LyricsFreak = 7,
        Lrclib = 8
    }
}
