# LSNET0004: removed LyricFind provider

**Obsolete since:** 3.0.0. **Removed in:** 4.0.0 for `WithLyricFind()` ([#81](https://github.com/skuill/LyricsScraperNET/issues/81)). The provider itself was removed in 3.0.0 ([#29](https://github.com/skuill/LyricsScraperNET/issues/29)).

## Members

| Obsolete | What to do |
|---|---|
| `LyricsScraperClientExtensions.WithLyricFind()` | Remove the call, it does nothing. Removed in 4.0.0. |
| `ExternalProviderType.LyricFind` (5) | Do not use it. A search request with it returns `ProviderNotAvailable`. The member stays: its number is part of the contract. |

## Why

Lyrics pages are behind a JavaScript bot challenge and cannot be loaded with plain HTTP requests.

Also remove the `LyricFindOptions` configuration section: it is ignored, and until 4.0.0 a warning is logged once.
