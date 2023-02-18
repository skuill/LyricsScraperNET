namespace LyricsScraperNET.Providers.Abstract
{
    public interface IExternalProviderLyricParser<T>
    {
        T Parse(string lyric);
    }
}
