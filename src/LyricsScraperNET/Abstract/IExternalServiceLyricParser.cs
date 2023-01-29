namespace LyricsScraperNET.Abstract
{
    public interface IExternalServiceLyricParser<T>
    {
        T Parse(string lyric);
    }
}
