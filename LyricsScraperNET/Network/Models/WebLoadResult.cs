using LyricsScraperNET.Network.Abstract;

namespace LyricsScraperNET.Network.Models
{
    /// <summary>
    /// The body of a page load together with how the load ended.
    /// </summary>
    /// <param name="Body">The body that <see cref="IWebClient.LoadAsync"/> returns for this response.</param>
    /// <param name="Status">How the load ended.</param>
    /// <param name="StatusCode">The HTTP status code, null if there was no response.</param>
    internal readonly record struct WebLoadResult(string Body, WebLoadStatus Status, int? StatusCode)
    {
        internal string FailureMessage
            => StatusCode is int statusCode ? $"HTTP {statusCode}" : "No response (network error or timeout)";
    }
}
