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
            // The site uses base letters: "Motörhead" is https://www.azlyrics.com/m/motorhead.html. "&" is dropped like on the site.
            var artistStripped = artist.RemoveDiacritics().ToLowerInvariant().StripRedundantChars(true);
            var titleStripped = song.RemoveDiacritics().ToLowerInvariant().StripRedundantChars();

            return new Uri(_baseUri, $"{artistStripped}/{titleStripped}.html");
        }
    }
}
