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
    }
}
