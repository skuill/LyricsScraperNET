using LyricsScraperNET.Providers.Models;
using System;
using System.Collections.Generic;

namespace LyricsScraperNET.Common
{
    internal static class Constants
    {
        public static Dictionary<ExternalProviderType, int> ProvidersSearchPriorities = new Dictionary<ExternalProviderType, int>()
        {
            { ExternalProviderType.AZLyrics, 6},
            { ExternalProviderType.Genius, 5},
            { ExternalProviderType.SongLyrics, 4},
            { ExternalProviderType.KPopLyrics, 2},
            { ExternalProviderType.LyricsFreak, 3},
            { ExternalProviderType.Lrclib, 7}
        };

        internal const string LibraryUserAgent = "LyricsScraperNET/1.0 (https://github.com/skuill/LyricsScraperNET)";

        /// <summary>
        /// Default maximum time one external provider gets to answer.
        /// </summary>
        internal static readonly TimeSpan DefaultProviderTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// The synchronous API is a blocking wrapper over the async one and is removed in 4.0.0 (#59, #79).
        /// </summary>
        internal static class ObsoleteSyncApi
        {
            internal const string DiagnosticId = "LSNET001";

            internal const string UrlFormat = "https://github.com/skuill/LyricsScraperNET/issues/79";

            internal const string SearchLyricMessage = "Blocking wrapper over SearchLyricAsync: it can deadlock in apps with a SynchronizationContext "
                + "(WinForms, WPF, MAUI, classic ASP.NET) and blocks thread pool threads. Use SearchLyricAsync. This member will be removed in 4.0.0.";

            internal const string LoadMessage = "Blocking wrapper over LoadAsync: it can deadlock in apps with a SynchronizationContext "
                + "and blocks thread pool threads. Use LoadAsync. This member will be removed in 4.0.0.";
        }

        internal static class ResponseMessages
        {
            internal static readonly string ExternalProvidersListIsEmpty = "Empty external providers list! Please set any external provider first";

            internal static readonly string ExternalProvidersAreDisabled = "All external providers are disabled. Searching lyrics is disabled";

            internal static readonly string NotFoundLyric = "Can't find lyric for the search request";

            internal static readonly string ExternalProviderForRequestNotSpecified = "The provider specified in the request is disabled or has not been configured for the client";

            internal static readonly string SearchRequestIsEmpty = "Search request is empty";

            internal static readonly string ArtistAndSongSearchRequestFieldsAreEmpty = "The ArtistAndSongSearchRequest not valid and contains one or more empty fields";

            internal static readonly string UriSearchRequestFieldsAreEmpty = "The UriSearchRequest not valid and contains one or more empty fields";
        }
    }
}
