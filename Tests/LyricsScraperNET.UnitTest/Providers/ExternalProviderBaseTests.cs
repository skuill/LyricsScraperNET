using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.Providers.AZLyrics;
using LyricsScraperNET.Providers.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers
{
    public class ExternalProviderBaseTests
    {
        [Theory]
        [MemberData(nameof(Requests))]
        public void SearchLyric_ProviderWithAsyncOverloadsOnly_ReturnsAsyncResult(SearchRequest searchRequest, string expectedLyric)
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var provider = new AsyncOnlyProvider();

            // Act
            var searchResult = provider.SearchLyric(searchRequest, cts.Token);

            // Assert
            Assert.Equal(expectedLyric, searchResult.LyricText);
            Assert.Equal(ResponseStatusCode.Success, searchResult.ResponseStatusCode);
            Assert.Equal(new[] { cts.Token }, provider.ReceivedTokens);
        }

        [Theory]
        [MemberData(nameof(Requests))]
        public void SearchLyric_ProviderWithOwnSyncOverloads_StillUsesThem(SearchRequest searchRequest, string expectedLyric)
        {
            // Arrange: a provider written for 2.x that overrides the obsolete sync overloads.
            var provider = new SyncOverridingProvider();

            // Act
            var searchResult = provider.SearchLyric(searchRequest, CancellationToken.None);

            // Assert
            Assert.Equal($"sync {expectedLyric}", searchResult.LyricText);
        }

        public static TheoryData<SearchRequest, string> Requests() => new()
        {
            { new ArtistAndSongSearchRequest("Artist", "Song"), "Artist - Song" },
            { new UriSearchRequest("https://lyrics.example/song"), "https://lyrics.example/song" },
        };

        private class AsyncOnlyProvider : ExternalProviderBase
        {
            public List<CancellationToken> ReceivedTokens { get; } = new();

            public override IExternalProviderOptions Options { get; } = new AZLyricsOptions { Enabled = true };

            protected override async Task<SearchResult> SearchLyricAsync(string artist, string song, CancellationToken cancellationToken = default)
            {
                ReceivedTokens.Add(cancellationToken);
                await Task.Yield();
                return new SearchResult($"{artist} - {song}", ExternalProviderType.AZLyrics);
            }

            protected override async Task<SearchResult> SearchLyricAsync(Uri uri, CancellationToken cancellationToken = default)
            {
                ReceivedTokens.Add(cancellationToken);
                await Task.Yield();
                return new SearchResult(uri.AbsoluteUri, ExternalProviderType.AZLyrics);
            }
        }

#pragma warning disable CS0672 // Overrides the obsolete sync overloads on purpose.
        private sealed class SyncOverridingProvider : AsyncOnlyProvider
        {
            protected override SearchResult SearchLyric(string artist, string song, CancellationToken cancellationToken = default)
                => new SearchResult($"sync {artist} - {song}", ExternalProviderType.AZLyrics);

            protected override SearchResult SearchLyric(Uri uri, CancellationToken cancellationToken = default)
                => new SearchResult($"sync {uri.AbsoluteUri}", ExternalProviderType.AZLyrics);
        }
#pragma warning restore CS0672
    }
}
