using LyricsScraperNET.Extensions;
using LyricsScraperNET.Providers.Abstract;
using System;

namespace LyricsScraperNET.Providers.SongLyrics
{
    internal sealed class SongLyricsUriConverter : IExternalUriConverter
    {
        // 0 - artist, 1 - song
        private const string uriPathFormat = "https://www.songlyrics.com/{0}/{1}-lyrics/";

        public Uri GetArtistUri(string artist)
        {
            throw new NotImplementedException();
        }

        public Uri GetLyricUri(string artist, string song)
        {
            // The site uses base letters: "Beyoncé" is https://www.songlyrics.com/beyonce/.
            var artistFormatted = artist.RemoveDiacritics().ToLowerInvariant().ConvertToDashedFormat();
            var songFormatted = song.RemoveDiacritics().ToLowerInvariant().ConvertToDashedFormat();

            return new Uri(string.Format(uriPathFormat, artistFormatted, songFormatted));
        }
    }
}
