using System;
using System.Threading.Tasks;

namespace LyricsScraperNET.Network.Abstract
{
    public interface IWebClient
    {
        string Load(Uri uri);

        Task<string> LoadAsync(Uri uri);
    }
}
