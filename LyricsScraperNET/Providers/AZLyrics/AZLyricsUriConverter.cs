using LyricsScraperNET.Extensions;
using LyricsScraperNET.Providers.Abstract;
using System;

namespace LyricsScraperNET.Providers.AZLyrics
{
    internal sealed class AZLyricsUriConverter : IExternalUriConverter
    {
        private Uri _baseUri => new Uri("http://www.azlyrics.com/lyrics/");

        public Uri GetArtistUri(string artist)
        {
            throw new NotImplementedException();
        }

        public Uri GetLyricUri(string artist, string song)
        {
            // remove articles from artist on start. For example for band [The Devil Wears Prada]: https://www.azlyrics.com/d/devilwearsprada.html
            var artistStripped = artist.ToLowerInvariant().StripRedundantChars(true);
            var titleStripped = song.ToLowerInvariant().StripRedundantChars();

            return new Uri(_baseUri, $"{artistStripped}/{titleStripped}.html");
        }
    }
}
