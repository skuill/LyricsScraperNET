using LyricsScraperNET.Providers.Abstract;
using System.Net;

namespace LyricsScraperNET.Providers.LyricsFreak
{
    internal sealed class LyricsFreakParser : IExternalProviderLyricParser
    {
        public string Parse(string lyric)
        {
            lyric = WebUtility.HtmlDecode(lyric);

            return lyric?.Trim() ?? string.Empty;
        }
    }
}
