using LyricsScraperNET.Providers.Abstract;
using System.Net;
using System.Text.RegularExpressions;

namespace LyricsScraperNET.Providers.Genius
{
    internal sealed class GeniusParser : IExternalProviderLyricParser
    {
        // HtmlAgilityPack writes "</br>" as "<br>".
        private static readonly Regex LineBreakRegex = new Regex(@"<br\s*/?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex TagRegex = new Regex("<[^>]*>", RegexOptions.Compiled);

        public string Parse(string lyric)
        {
            lyric = LineBreakRegex.Replace(lyric, "\r\n");
            lyric = TagRegex.Replace(lyric, string.Empty);
            lyric = WebUtility.HtmlDecode(lyric);

            return lyric.Trim();
        }
    }
}
