namespace LyricsScraperNET.Providers.Musixmatch
{
    /// <summary>
    /// Token cache provider
    /// </summary>
    public interface IMusixmatchTokenCache
    {
        /// <summary>
        /// Get or create token from cache. 
        /// </summary>
        /// <param name="regenerate">If true, then the token will be created again.</param>
        /// <returns></returns>
        string GetOrCreateToken(bool regenerate = false);
    }
}
