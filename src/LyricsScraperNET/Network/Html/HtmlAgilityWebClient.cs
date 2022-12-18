using HtmlAgilityPack;
using LyricsScraper.Network.Abstract;

namespace LyricsScraper.Network.Html
{
    public class HtmlAgilityWebClient : ILyricWebClient
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
