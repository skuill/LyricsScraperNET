using LyricsScraperNET.Common;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Genius;
using LyricsScraperNET.Providers.KPopLyrics;
using LyricsScraperNET.Providers.Lrclib;
using LyricsScraperNET.Providers.LyricsFreak;
using LyricsScraperNET.Providers.Models;
using LyricsScraperNET.Providers.SongLyrics;
using System;

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

        /// <summary>
        /// Does nothing. The Musixmatch provider was removed in 3.0.0.
        /// </summary>
        [Obsolete(Obsoletions.MusixmatchRemoved.WithMusixmatchMessage, DiagnosticId = Obsoletions.MusixmatchRemoved.DiagnosticId, UrlFormat = Obsoletions.SharedUrlFormat)]
        public static ILyricsScraperClient WithMusixmatch(this ILyricsScraperClient lyricsScraperClient)
            => lyricsScraperClient;

        public static ILyricsScraperClient WithSongLyrics(this ILyricsScraperClient lyricsScraperClient)
        {
            lyricsScraperClient.AddProvider(new SongLyricsProvider());
            return lyricsScraperClient;
        }

        /// <summary>
        /// Does nothing. The LyricFind provider was removed in 3.0.0.
        /// </summary>
        [Obsolete(Obsoletions.LyricFindRemoved.WithLyricFindMessage, DiagnosticId = Obsoletions.LyricFindRemoved.DiagnosticId, UrlFormat = Obsoletions.SharedUrlFormat)]
        public static ILyricsScraperClient WithLyricFind(this ILyricsScraperClient lyricsScraperClient)
            => lyricsScraperClient;

        public static ILyricsScraperClient WithKPopLyrics(this ILyricsScraperClient lyricsScraperClient)
        {
            lyricsScraperClient.AddProvider(new KPopLyricsProvider());
            return lyricsScraperClient;
        }

        public static ILyricsScraperClient WithLyricsFreak(this ILyricsScraperClient lyricsScraperClient)
        {
            lyricsScraperClient.AddProvider(new LyricsFreakProvider());
            return lyricsScraperClient;
        }

        public static ILyricsScraperClient WithLrclib(this ILyricsScraperClient lyricsScraperClient)
        {
            lyricsScraperClient.AddProvider(new LrclibProvider());
            return lyricsScraperClient;
        }

        /// <summary>
        /// Configure LyricsScraperClient with all available providers in <seealso cref="ExternalProviderType"/>.
        /// Search lyrics enabled by default for all providers.
        /// </summary>
        public static ILyricsScraperClient WithAllProviders(this ILyricsScraperClient lyricsScraperClient)
        {
            return lyricsScraperClient
                .WithGenius()
                .WithAZLyrics()
                .WithSongLyrics()
                .WithKPopLyrics()
                .WithLyricsFreak()
                .WithLrclib();
        }
    }
}
