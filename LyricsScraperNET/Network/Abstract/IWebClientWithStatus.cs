using LyricsScraperNET.Network.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.Network.Abstract
{
    /// <summary>
    /// Implemented by the library web clients, so a provider can tell "the site has no lyrics" from "the site could not be loaded".
    /// Internal: <see cref="IWebClient"/> stays unchanged for own implementations.
    /// </summary>
    internal interface IWebClientWithStatus : IWebClient
    {
        Task<WebLoadResult> LoadWithStatusAsync(Uri uri, CancellationToken cancellationToken = default);
    }
}
