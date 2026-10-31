# LSNET0002: ResponseStatusCode

**Obsolete since:** 3.0.0 ([#74](https://github.com/skuill/LyricsScraperNET/issues/74)). **Removed in:** 4.0.0 ([#82](https://github.com/skuill/LyricsScraperNET/issues/82)).

## Members

| Obsolete | Use instead |
|---|---|
| `ResponseStatusCode` (enum) | `SearchResultCode` |
| `SearchResult.ResponseStatusCode` | `SearchResult.Code` |

## Why

`ResponseStatusCode` does not tell the reason: `BadRequest` covers four different cases, `NoDataFound` does not say whether the providers answered or failed, `Error` was never returned and `RegionRestricted` has no provider since LyricFind was removed. `SearchResult.Code` gives the exact reason, and `SearchResult.ProviderAttempts` the outcome of each provider.

## How to migrate

| `ResponseStatusCode` | `SearchResult.Code` |
|---|---|
| `Success` | `Found`, `Instrumental` (`Code.IsSuccess`) |
| `NoDataFound` | `NotFound`, `NoProviders`, `AllProvidersDisabled` |
| `BadRequest` | `EmptyRequest`, `EmptyArtistOrSong`, `EmptyUri`, `ProviderNotAvailable` |
| `Error` | `AllProvidersFailed` |

```csharp
// Before
if (result.ResponseStatusCode == ResponseStatusCode.NoDataFound) { ... }

// After
if (result.Code == SearchResultCode.NotFound) { ... }
else if (result.Code == SearchResultCode.AllProvidersFailed) { ... } // retry later: the sites could not be checked
```

Until 4.0.0 `ResponseStatusCode` is derived from `Code` with the table above.
