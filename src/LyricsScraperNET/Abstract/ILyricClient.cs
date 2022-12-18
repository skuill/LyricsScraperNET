using LyricsScraper.Network.Abstract;

namespace LyricsScraper.Abstract
{
    public interface ILyricClient<T>
    {
        T SearchLyric(Uri uri);

        T SearchLyric(string artist, string song);

        Task<T> SearchLyricAsync(Uri uri);

        Task<T> SearchLyricAsync(string artist, string song);

        void WithParser(ILyricParser<T> parser);

        void WithWebClient(ILyricWebClient webClient);
    }
}
