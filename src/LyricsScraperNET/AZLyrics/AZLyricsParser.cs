using LyricsScraper.Abstract;
using LyricsScraper.Utils;

namespace LyricsScraper.AZLyrics
{
    public class AZLyricsParser : ILyricParser<string>
    {
        public string Parse(string lyric)
        {
            return UnescapeString(RemoveAllHtmlTags(lyric));
        }

        private string RemoveAllHtmlTags(string html)
        {
            html = StringUtils.RemoveHtmlTags(html);

            // fix recursive white-spaces
            while (html.Contains("  "))
            {
                html = html.Replace("  ", " ");
            }

            // fix recursive line-break
            while (html.Contains("\r\n\r\n\r\n"))
            {
                html = html.Replace("\r\n\r\n\r\n", "\r\n\r\n");
            }

            return html;
        }

        private string UnescapeString(string lyric)
        {
            if (!string.IsNullOrEmpty(lyric))
            {
                // replace entities with literal values
                lyric = lyric.Replace("&apos;", "'");
                lyric = lyric.Replace("&quot;", "\"");
                lyric = lyric.Replace("&gt;", ">");
                lyric = lyric.Replace("&lt;", "<");
                lyric = lyric.Replace("&amp;", "&");
            }
            return lyric;
        }
    }
}
