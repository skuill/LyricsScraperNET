using LyricsScraperNET.Extensions;
using LyricsScraperNET.Providers.Abstract;
using System;

namespace LyricsScraperNET.Providers.KPopLyrics
{
    internal sealed class KPopLyricsUriConverter : IExternalUriConverter
    {
        private Uri _baseUri => new Uri("https://www.kpoplyrics.net/");

        public Uri GetArtistUri(string artist)
        {
            throw new NotImplementedException();
        }

        public Uri GetLyricUri(string artist, string song)
        {
            return new Uri(_baseUri, $"{StringExtensions.CreateCombinedUrlSlug(artist, song)}.html");
        }
    }
}