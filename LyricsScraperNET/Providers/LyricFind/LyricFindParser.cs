using LyricsScraperNET.Extensions;
using LyricsScraperNET.Providers.Abstract;

namespace LyricsScraperNET.Providers.LyricFind
{
    internal sealed class LyricFindParser : IExternalProviderLyricParser
    {
        public string Parse(string lyric)
        {
            return lyric.RemoveAllHtmlTags()
                .UnescapeString()?
                .Trim()?
                .Replace("\\n", "\r\n")
                    ?? string.Empty;
        }
    }
}
