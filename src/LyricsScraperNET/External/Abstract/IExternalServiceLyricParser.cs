namespace LyricsScraperNET.External.Abstract
{
    public interface IExternalServiceLyricParser<T>
    {
        T Parse(string lyric);
    }
}
