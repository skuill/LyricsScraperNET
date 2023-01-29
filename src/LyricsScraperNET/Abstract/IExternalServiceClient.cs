using LyricsScraperNET.Network.Abstract;

namespace LyricsScraperNET.Abstract
{
    public interface IExternalServiceClient<T>
    {
        T SearchLyric(Uri uri);

        T SearchLyric(string artist, string song);

        Task<T> SearchLyricAsync(Uri uri);

        Task<T> SearchLyricAsync(string artist, string song);

        void WithParser(IExternalServiceLyricParser<T> parser);

        void WithWebClient(IWebClient webClient);
    }
}
