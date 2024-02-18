using LyricsScraperNET.Providers.Abstract;
using System;

namespace LyricsScraperNET.Providers.Genius
{
    internal sealed class GeniusUriConverter : IExternalUriConverter
    {
        // Format: "artist song". Example: "Parkway Drive Carrion".
        private const string GeniusSearchQueryFormat = "{0} {1}";
        private const string GeniusApiSearchFormat = "https://genius.com/api/search?q={0}";

        private string GetApiSearchQuery(string artist, string song)
            => string.Format(GeniusSearchQueryFormat, artist, song);

        public Uri GetLyricUri(string artist, string song)
            => new Uri(string.Format(GeniusApiSearchFormat, GetApiSearchQuery(artist, song)));
    }
}
