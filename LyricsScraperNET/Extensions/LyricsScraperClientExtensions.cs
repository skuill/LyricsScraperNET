using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Genius;
using LyricsScraperNET.Providers.Musixmatch;
using LyricsScraperNET.Providers.SongLyrics;

namespace LyricsScraperNET
{
    public static class LyricsScraperClientExtensions
    {
        public static ILyricsScraperClient WithAZLyrics(this ILyricsScraperClient lyricsScraperClient)
        {
            lyricsScraperClient.AddProvider(new AZLyricsProvider());
            return lyricsScraperClient;
        }

        public static ILyricsScraperClient WithGenius(this ILyricsScraperClient lyricsScraperClient)
        {
            lyricsScraperClient.AddProvider(new GeniusProvider());
            return lyricsScraperClient;
        }

        public static ILyricsScraperClient WithMusixmatch(this ILyricsScraperClient lyricsScraperClient)
        {
            lyricsScraperClient.AddProvider(new MusixmatchProvider());
            return lyricsScraperClient;
        }

        public static ILyricsScraperClient WithSongLyrics(this ILyricsScraperClient lyricsScraperClient)
        {
            lyricsScraperClient.AddProvider(new SongLyricsProvider());
            return lyricsScraperClient;
        }
    }
}
