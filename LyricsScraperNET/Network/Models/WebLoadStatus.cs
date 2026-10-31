namespace LyricsScraperNET.Network.Models
{
    /// <summary>
    /// How a page load ended, see <see cref="WebLoadResult"/>.
    /// </summary>
    internal enum WebLoadStatus
    {
        /// <summary>2xx response, the body may still be empty.</summary>
        Success,

        /// <summary>404: the site answered that the page does not exist.</summary>
        NotFound,

        /// <summary>The site could not be checked: network error, other unsuccessful status code, timeout of the client.</summary>
        Failed,
    }
}
