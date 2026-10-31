# LSNET0001: synchronous API

**Obsolete since:** 3.0.0. **Removed in:** 4.0.0 ([#79](https://github.com/skuill/LyricsScraperNET/issues/79)).

## Members

| Obsolete | Use instead |
|---|---|
| `ILyricsScraperClient.SearchLyric`, `LyricsScraperClient.SearchLyric` | `SearchLyricAsync` |
| `IExternalProvider.SearchLyric`, `ExternalProviderBase.SearchLyric` (public and the protected overloads) | `SearchLyricAsync` |
| `IWebClient.Load` | `LoadAsync` |

## Why

The sync methods block on the async ones (`GetAwaiter().GetResult()`). They block the calling thread and, with many concurrent calls, can starve the thread pool. Since 3.0.0 the library uses `ConfigureAwait(false)` everywhere, so they no longer deadlock in WinForms, WPF, MAUI or classic ASP.NET, but they still block ([#59](https://github.com/skuill/LyricsScraperNET/issues/59)).

## How to migrate

```csharp
// Before
var result = client.SearchLyric(request);

// After
var result = await client.SearchLyricAsync(request, cancellationToken);
```

Own providers: override only `SearchLyricAsync(Uri, CancellationToken)` and `SearchLyricAsync(string, string, CancellationToken)`. The sync overloads of `ExternalProviderBase` call them.
