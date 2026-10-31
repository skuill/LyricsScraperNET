using LyricsScraperNET.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET.Network.Abstract
{
    /// <summary>
    /// Defines methods for loading web resources from specified URIs.
    /// Contract for implementations:
    /// <list type="bullet">
    /// <item>return the response body on success;</item>
    /// <item>return an empty string on network errors, unsuccessful status codes, timeouts of the client itself and empty bodies, without throwing;</item>
    /// <item>throw <see cref="OperationCanceledException"/> only when <c>cancellationToken</c> is canceled.</item>
    /// </list>
    /// </summary>
    public interface IWebClient
    {
        /// <summary>
        /// Synchronously loads the content of the specified URI as a string.
        /// Obsolete: blocking wrapper over <see cref="LoadAsync"/>. Removed in 4.0.0.
        /// </summary>
        [Obsolete(Constants.ObsoleteSyncApi.LoadMessage, DiagnosticId = Constants.ObsoleteSyncApi.DiagnosticId, UrlFormat = Constants.ObsoleteSyncApi.UrlFormat)]
        string Load(Uri uri, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously loads the content of the specified URI as a string.
        /// </summary>
        Task<string> LoadAsync(Uri uri, CancellationToken cancellationToken = default);
    }
}
