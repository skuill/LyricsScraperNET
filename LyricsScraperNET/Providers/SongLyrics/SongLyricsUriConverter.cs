using LyricsScraperNET.Extensions;
using LyricsScraperNET.Providers.Abstract;
using System;

namespace LyricsScraperNET.Providers.SongLyrics
{
    internal sealed class SongLyricsUriConverter : IExternalUriConverter
    {
        // 0 - artist, 1 - song
        private const string uriPathFormat = "https://www.songlyrics.com/{0}/{1}-lyrics/";

        public Uri GetLyricUri(string artist, string song)
        {
            var artistFormatted = artist.ToLowerInvariant().СonvertToDashedFormat();
            var songFormatted = song.ToLowerInvariant().СonvertToDashedFormat();

            return new Uri(string.Format(uriPathFormat, artistFormatted, songFormatted));
        }
    }
}
