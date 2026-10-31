# Obsoletions

Every `[Obsolete]` member of LyricsScraperNET has its own diagnostic id and a page here. The compiler warning links to the page of its id.

| Id | What | Obsolete since | Removed in | Issue |
|---|---|---|---|---|
| [LSNET0001](LSNET0001.md) | Synchronous API (`SearchLyric`, `IWebClient.Load`) | 3.0.0 | 4.0.0 | [#79](https://github.com/skuill/LyricsScraperNET/issues/79) |
| [LSNET0002](LSNET0002.md) | `ResponseStatusCode` and `SearchResult.ResponseStatusCode` | 3.0.0 | 4.0.0 | [#82](https://github.com/skuill/LyricsScraperNET/issues/82) |
| [LSNET0003](LSNET0003.md) | Members kept for the removed Musixmatch provider | 3.0.0 | 4.0.0 (`WithMusixmatch()`) | [#81](https://github.com/skuill/LyricsScraperNET/issues/81) |
| [LSNET0004](LSNET0004.md) | Members kept for the removed LyricFind provider | 3.0.0 | 4.0.0 (`WithLyricFind()`) | [#81](https://github.com/skuill/LyricsScraperNET/issues/81) |

## Suppressing a warning

`CS0618` does not cover these ids. Suppress one id in the project file until you migrate:

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);LSNET0001</NoWarn>
</PropertyGroup>
```

or around the code that still uses the member:

```csharp
#pragma warning disable LSNET0001
var result = client.SearchLyric(request);
#pragma warning restore LSNET0001
```

## Rules for maintainers

- Ids `LSNET0001` to `LSNET0999` are for obsoletions. Take the next free number; never reuse or renumber an id, also after the member is removed.
- One id per obsoletion decision, for example one per removed provider.
- Messages and ids live in `LyricsScraperNET/Common/Obsoletions.cs`, one nested class per id. All attributes use `Obsoletions.SharedUrlFormat`.
- Every id has a page here with the same sections; the page stays after the member is removed (mark it "Removed in x.y.z").
- `ObsoletionsTests` checks that every `[Obsolete]` uses a known id, the shared URL and has a page here.
