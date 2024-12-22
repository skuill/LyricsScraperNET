using System;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.Network.Abstract
{
    /// <summary>
    /// Defines methods for loading web resources from specified URIs.
    /// </summary>
    public interface IWebClient
    {
        /// <summary>
        /// Synchronously loads the content of the specified URI as a string.
        /// </summary>
        string Load(Uri uri, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously loads the content of the specified URI as a string.
        /// </summary>
        Task<string> LoadAsync(Uri uri, CancellationToken cancellationToken = default);
    }
}
