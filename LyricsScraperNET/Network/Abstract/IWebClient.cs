using System;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.Network.Abstract
{
    public interface IWebClient
    {
        string Load(Uri uri, CancellationToken cancellationToken = default);

        Task<string> LoadAsync(Uri uri, CancellationToken cancellationToken = default);
    }
}
