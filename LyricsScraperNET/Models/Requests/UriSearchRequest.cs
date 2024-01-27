using LyricsScraperNET.Providers.Models;
using System;

namespace LyricsScraperNET.Models.Requests
{
    /// <summary>
    /// Query model for searching lyrics by the web address where the lyrics of one of the supported providers are located.
    /// </summary>
    public sealed class UriSearchRequest : SearchRequest
    {
        /// <summary>
        /// The web address where the lyrics of one of the supported providers are located.
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// The type of external provider for which lyrics will be searched.
        /// By default, it is set to <see cref="ExternalProviderType.None"/> - the search will be performed across all available client providers.
        /// </summary>
        public ExternalProviderType Provider { get; } = ExternalProviderType.None;

        public UriSearchRequest(Uri uri)
        {
            Uri = uri;
        }

        public UriSearchRequest(Uri uri, ExternalProviderType provider)
            : this(uri)
        {
            Provider = provider;
        }

        public UriSearchRequest(string uri) : this(new Uri(uri))
        { }

        public UriSearchRequest(string uri, ExternalProviderType provider) : this(new Uri(uri), provider)
        { }

        public override string ToString()
        {
            return $"Uri: [{Uri}]. Provider: [{Provider}]";
        }
    }
}
