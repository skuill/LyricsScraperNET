using LyricsScraperNET.Extensions;
using LyricsScraperNET.Providers.Abstract;
using System;

namespace LyricsScraperNET.Providers.LyricFind
{
    internal sealed class LyricFindUriConverter : IExternalUriConverter
    {
        // 0 - artist, 1 - song
        private const string uriPathFormat = "https://lyrics.lyricfind.com/lyrics/{0}-{1}";

        public Uri GetArtistUri(string artist)
        {
            throw new NotImplementedException();
        }

        public Uri GetLyricUri(string artist, string song)
        {
            var artistFormatted = artist.ToLowerInvariant().СonvertToDashedFormat(useExceptionSymbols: false, removeProhibitedSymbols: true);
            var songFormatted = song.ToLowerInvariant().СonvertToDashedFormat(useExceptionSymbols: false, removeProhibitedSymbols: true);

            return new Uri(string.Format(uriPathFormat, artistFormatted, songFormatted));
        }
    }
}
