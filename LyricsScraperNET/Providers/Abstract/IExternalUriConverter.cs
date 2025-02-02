using System;

namespace LyricsScraperNET.Providers.Abstract
{
    internal interface IExternalUriConverter
    {
        Uri GetLyricUri(string artist, string song);

        Uri GetArtistUri(string artist);
    }
}
