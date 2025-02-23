﻿using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsScraperNET
{
    public interface ILyricsScraperClient
    {
        /// <summary>
        /// Checking that there is some enabled external provider available for search.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Determines whether to use parallel search instead of sequential search. 
        /// If explicitly set, the value from the local variable is used. 
        /// Otherwise, it falls back to the configuration setting. 
        /// By default, sequential search is used if not specified.
        /// </summary>
        bool UseParallelSearch { get; set; }

        IExternalProvider? this[ExternalProviderType providerType] { get; }

        /// <summary>
        /// Search lyric by different search requests:
        /// 1) Search by Uri: <see cref="UriSearchRequest"/>
        /// 2) Search by Artist and Song name: <see cref="ArtistAndSongSearchRequest"/>
        /// 
        /// Supports cancellation via <see cref="CancellationToken"/>. 
        /// Throws <see cref="OperationCanceledException"/> if the operation is canceled.
        /// </summary>
        SearchResult SearchLyric(SearchRequest searchRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Async search lyric by different search requests:
        /// 1) Search by Uri: <see cref="UriSearchRequest"/>
        /// 2) Search by Artist and Song name: <see cref="ArtistAndSongSearchRequest"/>
        /// 
        /// Supports cancellation via <see cref="CancellationToken"/>. 
        /// Throws <see cref="OperationCanceledException"/> if the operation is canceled.
        /// </summary>
        Task<SearchResult> SearchLyricAsync(SearchRequest searchRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adding a new external provider that will be used to search for lyrics.
        /// </summary>
        void AddProvider(IExternalProvider provider);

        /// <summary>
        /// Removing an external provider by <paramref name="providerType"/> from the list of search providers.
        /// </summary>
        void RemoveProvider(ExternalProviderType providerType);

        /// <summary>
        /// Enable lyrics search. All external providers will be enabled in this case.
        /// </summary>
        void Enable();

        /// <summary>
        /// Disable lyrics search. All external providers will be enabled in this case.
        /// Calling the lyrics search method will return an empty result.
        /// </summary>
        void Disable();

        /// <summary>
        /// Creates a new ILogger instance from <paramref name="loggerFactory"/>.
        /// Can be useful for error analysis if the lyric text is not found.
        /// </summary>
        void WithLogger(ILoggerFactory loggerFactory);
    }
}
