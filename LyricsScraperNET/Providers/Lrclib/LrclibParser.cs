using LyricsScraperNET.Providers.Abstract;

namespace LyricsScraperNET.Providers.Lrclib
{
    internal sealed class LrclibParser : IExternalProviderLyricParser
    {
        public string Parse(string lyric)
        {
            return lyric?.Trim() ?? string.Empty;
        }
    }
}
