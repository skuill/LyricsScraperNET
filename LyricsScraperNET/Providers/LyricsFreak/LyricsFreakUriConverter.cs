using LyricsScraperNET.Extensions;
using LyricsScraperNET.Providers.Abstract;
using System;
using System.Linq;

namespace LyricsScraperNET.Providers.LyricsFreak
{
    internal sealed class LyricsFreakUriConverter : IExternalUriConverter
    {
        public const string BaseUrl = "https://www.lyricsfreak.com";

        // 0 - artist subgroup (first latin letter in artist's name), 1 - artist name
        private const string uriArtistPathFormat = BaseUrl + "/{0}/{1}";

        // Example for Artist parkway drive https://www.lyricsfreak.com/p/parkway+drive/
        public Uri GetArtistUri(string artist)
        {
            var artistFormatted = artist.ToLowerInvariant().СonvertToPlusFormat(removeProhibitedSymbols: true);
            return new Uri(string.Format(uriArtistPathFormat, artistFormatted.First(c => c != '+'), artistFormatted));

        }

        public Uri GetLyricUri(string artist, string song)
        {
            throw new NotImplementedException();
        }
    }
}
