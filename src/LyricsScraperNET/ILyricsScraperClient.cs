using LyricsScraper.Abstract;

namespace LyricsScraper
{
    public interface ILyricsScraperClient<T>
    {
        T SearchLyric(Uri uri);

        T SearchLyric(string artist, string song);

        Task<T> SearchLyricAsync(Uri uri);

        Task<T> SearchLyricAsync(string artist, string song);

        void AddClient(ILyricClient<T> client);
    }
}
