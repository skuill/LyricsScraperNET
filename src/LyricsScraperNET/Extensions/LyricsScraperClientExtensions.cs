using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Genius;

namespace LyricsScraperNET
{
    public static class LyricsScraperClientExtensions
    {
        public static ILyricsScraperClient<string> WithAZLyrics(this ILyricsScraperClient<string> lyricsScraperClient)
        {
            lyricsScraperClient.AddProvider(new AZLyricsProvider());
            return lyricsScraperClient;
        }

        public static ILyricsScraperClient<string> WithGenius(this ILyricsScraperClient<string> lyricsScraperClient)
        {
            lyricsScraperClient.AddProvider(new GeniusProvider());
            return lyricsScraperClient;
        }
    }
}
