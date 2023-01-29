using HtmlAgilityPack;
using LyricsScraperNET.Network.Abstract;

namespace LyricsScraperNET.Network.Html
{
    public sealed class HtmlAgilityWebClient : IWebClient
    {
        public string Load(Uri uri)
        {
            var htmlPage = new HtmlWeb();
            var document = htmlPage.Load(uri, "GET");

            return document?.ParsedText;
        }

        public async Task<string> LoadAsync(Uri uri)
        {
            var htmlPage = new HtmlWeb();
            var document = await htmlPage.LoadFromWebAsync(uri.ToString());

            return document?.ParsedText;
        }
    }
}
