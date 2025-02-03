using LyricsScraperNET.Extensions;
using LyricsScraperNET.Providers.Abstract;

namespace LyricsScraperNET.Providers.AZLyrics
{
    internal sealed class AZLyricsParser : IExternalProviderLyricParser
    {
        public string Parse(string lyric)
        {
            return lyric.RemoveAllHtmlTags()
                .UnescapeString()?
                .Trim()
                    ?? string.Empty;
        }
    }
}
