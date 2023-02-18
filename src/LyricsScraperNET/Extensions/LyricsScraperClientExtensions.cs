using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Genius;

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
    }
}
