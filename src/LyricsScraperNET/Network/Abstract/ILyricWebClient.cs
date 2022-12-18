namespace LyricsScraper.Network.Abstract
{
    public interface ILyricWebClient
    {
        string Load(Uri uri);

        Task<string> LoadAsync(Uri uri);
    }
}
