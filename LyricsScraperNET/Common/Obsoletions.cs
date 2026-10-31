namespace LyricsScraperNET.Common
{
    /// <summary>
    /// Messages and diagnostic ids of all <see cref="System.ObsoleteAttribute"/> usages, one nested class per obsoletion
    /// (attribute arguments must be constants). Rules (see <c>docs/obsoletions/README.md</c>):
    /// <list type="bullet">
    /// <item>ids <c>LSNET0001</c> to <c>LSNET0999</c> are for obsoletions; take the next free number, never reuse or renumber an id;</item>
    /// <item>one id per obsoletion decision (for example one per removed provider);</item>
    /// <item>every id has the page <c>docs/obsoletions/&lt;id&gt;.md</c>, kept after the member is removed;</item>
    /// <item>the message says what to use instead and in which version the member is removed.</item>
    /// </list>
    /// </summary>
    internal static class Obsoletions
    {
        /// <summary>
        /// The compiler replaces <c>{0}</c> with the diagnostic id.
        /// </summary>
        internal const string SharedUrlFormat = "https://github.com/skuill/LyricsScraperNET/blob/main/docs/obsoletions/{0}.md";

        /// <summary>
        /// LSNET0001: the synchronous API, a blocking wrapper over the async one. Obsolete in 3.0.0, removed in 4.0.0 (#59, #79).
        /// </summary>
        internal static class SyncApi
        {
            internal const string DiagnosticId = "LSNET0001";

            internal const string SearchLyricMessage = "Blocking wrapper over SearchLyricAsync: it can deadlock in apps with a SynchronizationContext "
                + "(WinForms, WPF, MAUI, classic ASP.NET) and blocks thread pool threads. Use SearchLyricAsync. This member will be removed in 4.0.0.";

            internal const string LoadMessage = "Blocking wrapper over LoadAsync: it can deadlock in apps with a SynchronizationContext "
                + "and blocks thread pool threads. Use LoadAsync. This member will be removed in 4.0.0.";
        }

        /// <summary>
        /// LSNET0002: <c>ResponseStatusCode</c>, replaced by <c>SearchResult.Code</c>. Obsolete in 3.0.0 (#74), removed in 4.0.0 (#82).
        /// </summary>
        internal static class ResponseStatusCode
        {
            internal const string DiagnosticId = "LSNET0002";

            internal const string Message = "Use SearchResult.Code (SearchResultCode): it tells the exact reason, for example NotFound or AllProvidersFailed. "
                + "ResponseStatusCode will be removed in 4.0.0.";
        }

        /// <summary>
        /// LSNET0003: members kept for the Musixmatch provider, removed in 3.0.0 (#54). <c>WithMusixmatch()</c> is removed in 4.0.0 (#81);
        /// the <c>ExternalProviderType.Musixmatch</c> member stays, its number is part of the contract.
        /// </summary>
        internal static class MusixmatchRemoved
        {
            internal const string DiagnosticId = "LSNET0003";

            internal const string WithMusixmatchMessage = "The Musixmatch provider was removed in LyricsScraperNET 3.0.0. This method does nothing and will be removed in 4.0.0.";

            internal const string ProviderTypeMessage = "The Musixmatch provider was removed in LyricsScraperNET 3.0.0. Searching with this provider type returns SearchResultCode.ProviderNotAvailable. "
                + "The member is kept so the numeric values of the other providers do not change.";
        }

        /// <summary>
        /// LSNET0004: members kept for the LyricFind provider, removed in 3.0.0 (#29). <c>WithLyricFind()</c> is removed in 4.0.0 (#81);
        /// the <c>ExternalProviderType.LyricFind</c> member stays, its number is part of the contract.
        /// </summary>
        internal static class LyricFindRemoved
        {
            internal const string DiagnosticId = "LSNET0004";

            internal const string WithLyricFindMessage = "The LyricFind provider was removed in LyricsScraperNET 3.0.0. This method does nothing and will be removed in 4.0.0.";

            internal const string ProviderTypeMessage = "The LyricFind provider was removed in LyricsScraperNET 3.0.0. Searching with this provider type returns SearchResultCode.ProviderNotAvailable. "
                + "The member is kept so the numeric values of the other providers do not change.";
        }
    }
}
