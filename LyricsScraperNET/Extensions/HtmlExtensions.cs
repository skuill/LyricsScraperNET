using HtmlAgilityPack;

namespace LyricsScraperNET.Extensions
{
    public static class HtmlExtensions
    {
        public static HtmlNode? SelectSingleNodeByXPath(this string htmlBody, string xPath)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlBody);
            return htmlDoc.DocumentNode.SelectSingleNode(xPath);
        }

        public static string RemoveAllHtmlTags(this string html)
        {
            html = html.RemoveHtmlTags();

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

        public static string UnescapeString(this string html)
        {
            if (!string.IsNullOrEmpty(html))
            {
                // replace entities with literal values
                html = html.Replace("&apos;", "'");
                html = html.Replace("&quot;", "\"");
                html = html.Replace("&gt;", ">");
                html = html.Replace("&lt;", "<");
                html = html.Replace("&amp;", "&");
            }
            return html;
        }
    }
}
