using LyricsScraperNET.Extensions;
using LyricsScraperNET.Providers.Abstract;
using System;
using System.Collections.Generic;
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
            return GetArtistUris(artist).FirstOrDefault()
                ?? throw new ArgumentException($"Artist name without Latin letters or digits: [{artist}].", nameof(artist));
        }

        /// <summary>
        /// Artist page candidates in the order to try. The site mostly uses base letters ("Beyoncé" is /b/beyonce/, "Ólafur Arnalds" is /o/olafur+arnalds/),
        /// but some artists keep the slug with the letters dropped ("Zé Ramalho" is /z/z+ramalho/, /z/ze+ramalho/ is 404).
        /// Empty for names without Latin letters or digits: the site has no page for them.
        /// </summary>
        public IReadOnlyList<Uri> GetArtistUris(string artist)
        {
            var uris = new List<Uri>();
            foreach (var name in new[] { artist.RemoveDiacritics(), artist })
            {
                var artistFormatted = name.ToLowerInvariant().ConvertToPlusFormat(removeProhibitedSymbols: true);
                var subgroup = artistFormatted?.FirstOrDefault(c => c != '+') ?? default;
                if (subgroup == default)
                    continue;

                var uri = new Uri(string.Format(uriArtistPathFormat, subgroup, artistFormatted));
                if (!uris.Contains(uri))
                    uris.Add(uri);
            }

            return uris;
        }

        public Uri GetLyricUri(string artist, string song)
        {
            throw new NotImplementedException();
        }
    }
}
