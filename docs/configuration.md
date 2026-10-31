# Configuration

LyricsScraperNET can be set up in code (fluent API) or from `IConfiguration` (for example `appsettings.json`) through dependency injection. Both ways use the same settings.

## Client settings

| Setting | Type | Default | Meaning |
|---|---|---|---|
| `UseParallelSearch` | `bool` | `false` | `false`: providers are asked one by one, from the highest `SearchPriority` down, until one finds the lyrics. `true`: all providers start at once, see [Search order](#search-order). |
| `ProviderTimeout` | `TimeSpan` | `00:00:10` | Maximum time one provider gets to answer. A provider that does not answer in time is canceled, logged as a warning and treated as "not found". `Timeout.InfiniteTimeSpan` disables the timeout, zero or negative values throw `ArgumentOutOfRangeException`. |

## Provider settings

Every provider has its own options class with the same base settings:

| Setting | Type | Meaning |
|---|---|---|
| `Enabled` | `bool` | A disabled provider is skipped. |
| `SearchPriority` | `int` | Higher value means the provider is asked earlier. |

Default priorities:

| Provider | Options class / section | Default `SearchPriority` | Extra settings |
|---|---|---|---|
| LRCLIB | `LrclibOptions` | 7 | |
| AZLyrics | `AZLyricsOptions` | 6 | |
| Genius | `GeniusOptions` | 5 | `ApiKey` (optional), see [providers.md](providers.md#genius) |
| SongLyrics | `SongLyricsOptions` | 4 | |
| LyricsFreak | `LyricsFreakOptions` | 3 | |
| KPopLyrics | `KPopLyricsOptions` | 2 | |

## Configuration in code

```csharp
// All providers with default settings.
ILyricsScraperClient client = new LyricsScraperClient()
    .WithAllProviders();

// Only some providers.
ILyricsScraperClient client = new LyricsScraperClient()
    .WithGenius()
    .WithLrclib();

// A provider with own options.
var client = new LyricsScraperClient();
client.AddProvider(new GeniusProvider(new GeniusOptions
{
    Enabled = true,
    SearchPriority = 10,
    ApiKey = "<your Genius client access token>",
}));

// Client settings.
client.UseParallelSearch = true;
client.ProviderTimeout = TimeSpan.FromSeconds(5);
```

Providers can be added, removed, enabled and disabled at any time, also while searches are running: `AddProvider`, `RemoveProvider`, `Enable()` / `Disable()` on the client or on a provider. `client[ExternalProviderType.Genius]` returns the provider of that type.

## Configuration file

The section is called `LyricScraperClient`. With dependency injection only the providers whose section is present are registered; see [dependency-injection.md](dependency-injection.md).

```json
{
  "LyricScraperClient": {
    "UseParallelSearch": false,
    "ProviderTimeout": "00:00:10",
    "GeniusOptions": {
      "SearchPriority": 5,
      "Enabled": true,
      "ApiKey": ""
    },
    "AZLyricsOptions": {
      "SearchPriority": 6,
      "Enabled": true
    },
    "SongLyricsOptions": {
      "SearchPriority": 4,
      "Enabled": true
    },
    "KPopLyricsOptions": {
      "SearchPriority": 2,
      "Enabled": true
    },
    "LyricsFreakOptions": {
      "SearchPriority": 3,
      "Enabled": true
    },
    "LrclibOptions": {
      "SearchPriority": 7,
      "Enabled": true
    }
  }
}
```

Keep the Genius `ApiKey` out of source control, for example in user secrets or an environment variable (`LyricScraperClient__GeniusOptions__ApiKey`).

Sections of removed providers (`MusixmatchOptions`, `LyricFindOptions`) are ignored; if logging is configured, the library writes a warning once.

## Search order

- **Sequential** (`UseParallelSearch = false`): providers are asked one after another, highest `SearchPriority` first. The first provider that finds the lyrics (or reports an instrumental song) ends the search.
- **Parallel** (`UseParallelSearch = true`): all providers start at once. A result is returned as soon as no provider with a higher priority is still running; among providers with the same priority the fastest one wins. The remaining providers are canceled.

In both modes a request for one provider is sent only to that provider: `new ArtistAndSongSearchRequest(artist, song, ExternalProviderType.Genius)` or `new UriSearchRequest(uri, ExternalProviderType.Genius)`.

## Search requests and results

```csharp
var byArtistAndSong = new ArtistAndSongSearchRequest("John Newton", "Amazing Grace");
var byUri = new UriSearchRequest("https://genius.com/John-newton-amazing-grace-lyrics");

SearchResult result = await client.SearchLyricAsync(byArtistAndSong, cancellationToken);
```

| `SearchResult` member | Meaning |
|---|---|
| `LyricText` | The lyrics, empty if nothing was found or the song is instrumental. |
| `IsEmpty()` | `true` if `LyricText` is empty. |
| `Instrumental` | `true` if the provider reports a song without lyrics. |
| `ExternalProviderType` | The provider that answered. |
| `ResponseStatusCode` | `Success`, `NoDataFound`, `BadRequest`, ... |
| `ResponseMessage` | A human readable reason for an unsuccessful search. |

Canceling `cancellationToken` throws `OperationCanceledException` right away.

The sync `SearchLyric` blocks the calling thread until the search ends. Prefer `SearchLyricAsync`, especially in UI apps and in services with many concurrent searches.

## Logging

```csharp
client.WithLogger(loggerFactory);
```

With dependency injection the client and providers get their loggers from the container.
