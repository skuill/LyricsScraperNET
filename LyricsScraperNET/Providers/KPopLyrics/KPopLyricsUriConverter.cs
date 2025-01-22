using System;
using LyricsScraperNET.Extensions;
using LyricsScraperNET.Providers.Abstract;

namespace LyricsScraperNET.Providers.KPopLyrics
{
    public class KPopLyricsUriConverter : IExternalUriConverter
    {
        private Uri _baseUri => new Uri("https://www.kpoplyrics.net/");

        public Uri GetLyricUri(string artist, string song)
        {
            return new Uri(_baseUri, $"{StringExtensions.CreateCombinedUrlSlug(artist, song)}.html");
        }
    }
}