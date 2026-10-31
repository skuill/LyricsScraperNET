using LyricsScraperNET.Providers.Abstract;
using System;

namespace LyricsScraperNET.Providers.Genius
{
    internal sealed class GeniusUriConverter : IExternalUriConverter
    {
        /// <summary>
        /// Host of the official API: requests to it are sent with the API key.
        /// </summary>
        internal const string ApiHost = "api.genius.com";

        // The public search of the site, works without an API key.
        private const string SearchUriFormat = "https://genius.com/api/search?q={0}";

        // The official API search, needs an API key. Returns the same JSON as the public search.
        private const string ApiSearchUriFormat = "https://" + ApiHost + "/search?q={0}";

        /// <summary>
        /// The public search for "artist song". Example: "Parkway Drive Carrion".
        /// </summary>
        public Uri GetLyricUri(string artist, string song)
            => GetSearchUri(SearchUriFormat, artist, song);

        /// <summary>
        /// The official API search for "artist song".
        /// </summary>
        public Uri GetApiSearchUri(string artist, string song)
            => GetSearchUri(ApiSearchUriFormat, artist, song);

        public Uri GetArtistUri(string artist)
        {
            throw new NotImplementedException();
        }

        private static Uri GetSearchUri(string format, string artist, string song)
            => new Uri(string.Format(format, Uri.EscapeDataString($"{artist} {song}")));
    }
}
