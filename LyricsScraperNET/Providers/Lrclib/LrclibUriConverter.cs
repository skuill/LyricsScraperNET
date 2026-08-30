using LyricsScraperNET.Providers.Abstract;
using System;

namespace LyricsScraperNET.Providers.Lrclib
{
    internal sealed class LrclibUriConverter : IExternalUriConverter
    {
        internal const string BaseApiUrl = "https://lrclib.net/api/get";

        public Uri GetArtistUri(string artist)
        {
            throw new NotImplementedException();
        }

        public Uri GetLyricUri(string artist, string song)
        {
            var query = $"artist_name={Uri.EscapeDataString(artist)}&track_name={Uri.EscapeDataString(song)}";
            return new Uri($"{BaseApiUrl}?{query}");
        }
    }
}
