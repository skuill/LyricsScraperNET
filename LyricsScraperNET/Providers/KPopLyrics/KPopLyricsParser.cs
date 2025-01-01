using System.Linq;
using HtmlAgilityPack;
using LyricsScraperNET.Providers.Abstract;

namespace LyricsScraperNET.Providers.KPopLyrics
{
    public class KPopLyricsParser : IExternalProviderLyricParser
    {
        public string Parse(string lyric)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(lyric);

            var deEntitizedText = string.Join("\n\n", // <p> -> \n\n
                htmlDoc.DocumentNode.SelectNodes("//p")
                    .Select(node => HtmlEntity.DeEntitize(node.InnerHtml
                        .Replace("<br> ", "\n") // the trailing whitespace after <br> is necessary
                        .Trim()
                    )));

            return deEntitizedText;
        }
    }
}