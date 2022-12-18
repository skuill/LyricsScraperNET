namespace LyricsScraper.Abstract
{
    public interface ILyricParser<T>
    {
        T Parse(string lyric);
    }
}
