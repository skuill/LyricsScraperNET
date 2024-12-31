using LyricsScraperNET.Extensions;
using LyricsScraperNET.Providers.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.LyricsFreak
{
    internal sealed class LyricsFreakUriConverter : IExternalUriConverter
    {
        public const string BaseUrl = "https://www.lyricsfreak.com";
        // 0 - artist, 1 - song
        private const string uriArtistPathFormat = BaseUrl + "/{0}/{1}";
        public Uri GetLyricUri(string artist, string song)
        {
            var artistFormatted = artist.ToLowerInvariant().СonvertSpaceToPlusFormat(removeProhibitedSymbols: true);
            return GetArtistUri(artistFormatted);
        }
        // Example for Artist parkway drive https://www.lyricsfreak.com/p/parkway+drive/
        private static Uri GetArtistUri(string artist)
        {
            return new Uri(string.Format(uriArtistPathFormat, artist.Length > 0 ? artist[0] : string.Empty, artist));

        }
    }
}
