using LyricsScraperNET.Providers.Abstract;
using System.Net;

namespace LyricsScraperNET.Providers.SongLyrics
{
    internal sealed class SongLyricsParser : IExternalProviderLyricParser
    {
        public string Parse(string lyric)
        {
            lyric = WebUtility.HtmlDecode(lyric);

            return lyric?.Trim();
        }
    }
}
