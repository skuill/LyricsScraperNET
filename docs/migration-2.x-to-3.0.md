# Migrating from 2.x to 3.0

3.0 is a major release: it drops old target frameworks, removes two broken providers and changes how the search and HTTP work. Most applications only need the [checklist](#checklist); the sections below explain each change.

## Checklist

1. Make sure the application runs on .NET 8, 9 or 10.
2. Remove `MusixmatchOptions` and `LyricFindOptions` from the configuration.
3. Remove calls to `WithMusixmatch()` and `WithLyricFind()`, uses of `ExternalProviderType.Musixmatch` / `ExternalProviderType.LyricFind`, and of the removed types and properties listed in [Providers](#providers) (`MusixmatchProvider`, `LyricScraperClientConfig.MusixmatchOptions`, ...).
4. If your code used `Genius.NET`, `Jil`, `Sigil`, `MusixmatchClientLib` or `Microsoft.Extensions.Caching.*` through LyricsScraperNET, reference these packages directly.
5. If you implement `ILyricsScraperClient` or `ILyricScraperClientConfig` yourself (not a mock), add the new members.
6. If you implement `IWebClient` yourself, follow the [new contract](#web-client-contract).
7. Check the Genius searches that relied on a "close enough" result, see [Genius](#genius).
8. Replace the sync `SearchLyric` and `IWebClient.Load` calls with `SearchLyricAsync` and `LoadAsync`: they are obsolete ([LSNET0001](obsoletions/LSNET0001.md)) and will be removed in 4.0.0, see [Sync API](#sync-api).
9. Replace `ResponseStatusCode` with `SearchResult.Code` ([LSNET0002](obsoletions/LSNET0002.md)), see [Search result](#search-result). Note that a search where no provider could answer now returns `AllProvidersFailed` (`ResponseStatusCode.Error`) instead of `NoDataFound`.
10. If your code used `HtmlExtensions`, `ProviderService`, `IProviderService`, `RequestValidator`, `IRequestValidator` or `SearchRequest.IsValid`, see [Internal types](#internal-types).
11. Optional: set `ProviderTimeout`, your own `HttpClient` or User-Agent ([configuration.md](configuration.md), [http.md](http.md)).

All obsolete members, their diagnostic ids and replacements: [obsoletions](obsoletions/README.md).

## Target frameworks

| 2.x | 3.0 |
|---|---|
| `net8.0`, `net7.0`, `net6.0`, `net5.0`, `netstandard2.1`, `netstandard2.0` | `net10.0`, `net9.0`, `net8.0` |

Applications on .NET Framework or .NET 5 to 7 stay on 2.x.

## Dependencies

| Package | Change |
|---|---|
| `Microsoft.Extensions.*` | 6.0 to the version of the target framework: 8.0.x for net8.0, 9.0.x for net9.0, 10.0.x for net10.0 |
| `Microsoft.Extensions.Http` | Added (`IHttpClientFactory` with dependency injection) |
| `HtmlAgilityPack` | 1.11 to 1.13 |
| `MusixmatchClientLib` | Removed with the Musixmatch provider |
| `Genius.NET`, `Sigil` (and `Jil` through `Genius.NET`) | Removed, Genius uses the library `HttpClient` and `System.Text.Json` |
| `Microsoft.Extensions.Caching.Abstractions`, `Microsoft.Extensions.Caching.Memory` | Removed (not used) |
| `System.Text.Json` | Removed as a package, it is part of .NET |

## Providers

| Provider | Change | Issue |
|---|---|---|
| Musixmatch | Removed: the site serves decoy lyrics to non-authorized clients. | [#54](https://github.com/skuill/LyricsScraperNET/issues/54) |
| LyricFind | Removed: lyrics pages are behind a JavaScript bot challenge. | [#29](https://github.com/skuill/LyricsScraperNET/issues/29) |
| LRCLIB | New provider, searched first by default (`SearchPriority` 7). | [#52](https://github.com/skuill/LyricsScraperNET/issues/52) |
| SongLyrics | Fixed after the site redesign. | [#55](https://github.com/skuill/LyricsScraperNET/issues/55) |
| Genius | See [Genius](#genius). | [#61](https://github.com/skuill/LyricsScraperNET/issues/61) |

Removed with them:

| Removed | Notes |
|---|---|
| `MusixmatchProvider`, `MusixmatchOptions`, `MusixmatchClientWrapper`, `IMusixmatchClientWrapper`, `MusixmatchTokenCache`, `IMusixmatchTokenCache` | Namespace `LyricsScraperNET.Providers.Musixmatch` |
| `LyricFindProvider`, `LyricFindOptions` | Namespace `LyricsScraperNET.Providers.LyricFind` |
| `MusixmatchOptions` and `LyricFindOptions` properties of `ILyricScraperClientConfig` and `LyricScraperClientConfig` | The configuration sections are ignored, see below |

What stays for compatibility:
- `ExternalProviderType.Musixmatch` (3) and `ExternalProviderType.LyricFind` (5) are `[Obsolete]` members, so the numbers of the other providers do not change. A search request for them returns `BadRequest`.
- `WithMusixmatch()` and `WithLyricFind()` are `[Obsolete]` and do nothing.
- Configuration sections `MusixmatchOptions` and `LyricFindOptions` are ignored. If logging is configured, the library writes a warning once.

## Search

- **Provider timeout:** every provider gets at most `ProviderTimeout` (default 10 seconds) to answer. A slow provider is treated as "not found" and the search continues. 2.x waited as long as the provider took. ([#58](https://github.com/skuill/LyricsScraperNET/issues/58), [#51](https://github.com/skuill/LyricsScraperNET/issues/51))
- **Parallel search** prefers priority over speed: a result is returned once no provider with a higher `SearchPriority` is still running. In 2.x the fastest provider won. ([#57](https://github.com/skuill/LyricsScraperNET/issues/57))
- **Cancellation:** canceling the token throws `OperationCanceledException` right away and cancels the providers. The sync `SearchLyric` throws the `OperationCanceledException` itself instead of a `TaskCanceledException` wrapper.
- **Thread safety:** providers can be added and removed while searches are running.

## HTTP

- All providers share one long-lived `HttpClient`; 2.x created HTTP handlers per request. ([#60](https://github.com/skuill/LyricsScraperNET/issues/60))
- Requests use the User-Agent `LyricsScraperNET/1.0 (https://github.com/skuill/LyricsScraperNET)` instead of a browser one, unless your client sets its own.
- Error pages (unsuccessful status codes) are no longer parsed as lyrics pages.
- New: `ILyricsScraperClient.WithHttpClient(HttpClient)`, `ExternalProviderBase.WithHttpClient(HttpClient)` and the named client `ServiceCollectionExtensions.HttpClientName` for dependency injection. See [http.md](http.md).

### Web client contract

`IWebClient` (used with `ExternalProviderBase.WithWebClient`) now has a documented contract:
- return the response body on success;
- return an empty string on network errors, unsuccessful status codes, timeouts of the client itself and empty bodies, without throwing;
- throw `OperationCanceledException` only when the caller's token is canceled.

## Genius

- **Exact hit:** a search by artist and song returns lyrics only for a hit whose artist and title match the request. 2.x took the first hit of the search, which could be another song, a live version, a remix or a translation page. Case, diacritics and punctuation are still ignored, so `Beyonce` / `Halo` finds `Beyoncé` / `Halo`. Rules and examples: [providers.md](providers.md#genius).
- **Special characters:** `&`, `#`, `?` and `/` in artist or song names no longer break the search query.
- **API key:** the search with `GeniusOptions.ApiKey` goes through the library `HttpClient`; the key is sent only to `api.genius.com`. If the API request fails, the provider falls back to the public search; if the API answers without a matching hit, the result is `NoDataFound` without a second request.
- **Errors:** a page without lyrics returns `NoDataFound` instead of throwing `NullReferenceException`.

## Sync API

The synchronous API is a blocking wrapper over the async one. In 3.0 it is marked `[Obsolete]` with the diagnostic id [LSNET0001](obsoletions/LSNET0001.md) and will be removed in 4.0.0 ([#79](https://github.com/skuill/LyricsScraperNET/issues/79)). ([#59](https://github.com/skuill/LyricsScraperNET/issues/59))

| Obsolete member | Use instead |
|---|---|
| `ILyricsScraperClient.SearchLyric` | `SearchLyricAsync` |
| `IExternalProvider.SearchLyric`, `ExternalProviderBase.SearchLyric` (public and the protected overloads) | `SearchLyricAsync` |
| `IWebClient.Load` | `LoadAsync` |

- The library no longer captures the caller's `SynchronizationContext` (`ConfigureAwait(false)` on every `await`), so the sync API does not deadlock in WinForms, WPF, MAUI or classic ASP.NET. It still blocks the calling thread.
- Own providers: override only the async overloads `SearchLyricAsync(Uri, CancellationToken)` and `SearchLyricAsync(string, string, CancellationToken)`. The sync overloads of `ExternalProviderBase` call them. Providers that still override the sync overloads keep working until 4.0.0 (compiler warning `CS0672`).
- To silence the warning until you migrate: `<NoWarn>$(NoWarn);LSNET0001</NoWarn>` in the project file.

## Search result

`SearchResult` tells why a search ended and what each provider did ([#74](https://github.com/skuill/LyricsScraperNET/issues/74)). Details: [configuration.md](configuration.md#search-requests-and-results).

- New `SearchResult.Code` (`SearchResultCode`): `Found`, `Instrumental`, `NotFound`, `AllProvidersFailed`, `EmptyRequest`, `EmptyArtistOrSong`, `EmptyUri`, `ProviderNotAvailable`, `NoProviders`, `AllProvidersDisabled`.
- New `SearchResult.ProviderAttempts`: provider, outcome (`Found`, `Instrumental`, `NotFound`, `Failed`, `Timeout`, `Error`, `Canceled`), duration and message.
- `ResponseStatusCode` and `SearchResult.ResponseStatusCode` are obsolete ([LSNET0002](obsoletions/LSNET0002.md)) and derived from `Code`.
- **Behavior change:** if every provider failed (site unavailable, timeout or error), the result is `AllProvidersFailed` with `ResponseStatusCode.Error`. In 2.x it was `NoDataFound`, so "the sites could not be checked" looked like "the song does not exist".
- **Behavior change:** a site that answers with an error status (for example `403`, `429`, `503`) is reported as `Failed`, not as "not found". A `404` page still counts as "not found".
- `SearchResult.Empty` returns a new instance on every call (it was one shared instance).
- The client returns a copy of the provider result with `ProviderAttempts` filled; the object returned by the provider is not changed.
- For tests: `LyricsScraperModelFactory` creates `SearchResult` and `ProviderAttempt` instances (their constructors are internal).

## Internal types

These types and members were public by accident: the client creates them itself and they cannot be replaced. In 3.0 they are `internal`.

| Type or member | Replacement |
|---|---|
| `HtmlExtensions` (`SelectSingleNodeByXPath`, `RemoveAllHtmlTags`, `UnescapeString`) | HtmlAgilityPack directly |
| `ProviderService`, `IProviderService` | `ILyricsScraperClient` (`AddProvider`, `RemoveProvider`, indexer) |
| `RequestValidator`, `IRequestValidator` | `SearchResult.Code` of the search |
| `SearchRequest.IsValid(out string)` | `SearchResult.Code` of the search. `SearchRequest` can no longer be derived from outside the library: use `ArtistAndSongSearchRequest` or `UriSearchRequest`. |

## Interfaces

New members, breaking only for your own implementations (mocks are not affected):

| Interface | New member |
|---|---|
| `ILyricsScraperClient` | `TimeSpan ProviderTimeout { get; set; }` |
| `ILyricsScraperClient` | `ILyricsScraperClient WithHttpClient(HttpClient httpClient)` |
| `ILyricScraperClientConfig` | `TimeSpan? ProviderTimeout { get; }` |
| `ILyricScraperClientConfig` | `IExternalProviderOptions LrclibOptions { get; }` (LRCLIB provider) |

Own providers derived from `ExternalProviderBase`: `Options` is now `abstract` (it threw `NotImplementedException` when not overridden), so a provider without `Options` fails at compile time instead of at the first search.

The complete list of API changes against 2.2.2 was checked by Package Validation and is recorded in [`LyricsScraperNET/CompatibilitySuppressions.xml` of 3.0.0](https://github.com/skuill/LyricsScraperNET/blob/3.0.0/LyricsScraperNET/CompatibilitySuppressions.xml).
