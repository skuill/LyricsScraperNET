using LyricsScraperNET.Providers.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
