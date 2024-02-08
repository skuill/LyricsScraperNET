using LyricsScraperNET.Models.Responses;
using System.Threading.Tasks;

namespace LyricsScraperNET.Providers.Musixmatch
{
    /// <summary>
    /// Decorator for the <seealso cref="MusixmatchClientLib.MusixmatchClient"/>
    /// </summary>
    public interface IMusixmatchClientWrapper
    {
        SearchResult SearchLyric(string artist, string song, bool regenerateToken = false);

        Task<SearchResult> SearchLyricAsync(string artist, string song, bool regenerateToken = false);
    }
}
