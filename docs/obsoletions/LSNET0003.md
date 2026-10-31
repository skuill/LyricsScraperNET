# LSNET0003: removed Musixmatch provider

**Obsolete since:** 3.0.0. **Removed in:** 4.0.0 for `WithMusixmatch()` ([#81](https://github.com/skuill/LyricsScraperNET/issues/81)). The provider itself was removed in 3.0.0 ([#54](https://github.com/skuill/LyricsScraperNET/issues/54)).

## Members

| Obsolete | What to do |
|---|---|
| `LyricsScraperClientExtensions.WithMusixmatch()` | Remove the call, it does nothing. Removed in 4.0.0. |
| `ExternalProviderType.Musixmatch` (3) | Do not use it. A search request with it returns `ProviderNotAvailable`. The member stays: its number is part of the contract. |

## Why

The site serves decoy lyrics to non-authorized clients, so the provider returned wrong text for any query.

Also remove the `MusixmatchOptions` configuration section: it is ignored, and until 4.0.0 a warning is logged once.
