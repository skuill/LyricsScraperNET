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

The section is called `LyricScraperClient`. Each provider has its own section where it can be enabled, disabled and prioritized. The file works in two ways:

- with dependency injection only the providers whose section is present are registered, see [dependency-injection.md](dependency-injection.md);
- without dependency injection the section is read into `LyricScraperClientConfig` and the providers are created by hand, see [Configuration without dependency injection](#configuration-without-dependency-injection).

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

## Configuration without dependency injection

The same file, read by hand. `LyricScraperClientConfig` has one property per provider (`GeniusOptions`, `AZLyricsOptions`, ...) that is filled from the provider sections:

```csharp
IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var clientConfig = configuration
    .GetSection(LyricScraperClientConfig.ConfigurationSectionName)
    .Get<LyricScraperClientConfig>() ?? new LyricScraperClientConfig();

IExternalProvider[] providers =
{
    new GeniusProvider((GeniusOptions)clientConfig.GeniusOptions),
    new AZLyricsProvider((AZLyricsOptions)clientConfig.AZLyricsOptions),
    new SongLyricsProvider((SongLyricsOptions)clientConfig.SongLyricsOptions),
    new KPopLyricsProvider((KPopLyricsOptions)clientConfig.KPopLyricsOptions),
    new LyricsFreakProvider((LyricsFreakOptions)clientConfig.LyricsFreakOptions),
    new LrclibProvider((LrclibOptions)clientConfig.LrclibOptions),
};

// UseParallelSearch and ProviderTimeout come from the section; providers with "Enabled": false are skipped.
ILyricsScraperClient client = new LyricsScraperClient(clientConfig, providers);
```

A provider without a section keeps its default options, which are disabled (`Enabled` is `false`). The full example is `ExampleWithConfigurationWithoutDependencyInjection` in [LyricsScraperNET.Client/Program.cs](../LyricsScraperNET.Client/Program.cs).

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
| `Code` | Why the search ended, see the table below. |
| `LyricText` | The lyrics, empty if nothing was found or the song is instrumental. |
| `IsEmpty()` | `true` if `LyricText` is empty. |
| `Instrumental` | `true` for `Code` `Instrumental`. |
| `ExternalProviderType` | The provider that answered. |
| `ProviderAttempts` | What each started provider did: provider, `Outcome`, `Duration`, `Message`. |
| `ResponseMessage` | A human readable text. Use `Code` in code. |
| `ResponseStatusCode` | Obsolete ([LSNET0002](obsoletions/LSNET0002.md)), derived from `Code`. |

| `Code` | Meaning | Typical HTTP mapping in an API |
|---|---|---|
| `Found` | The lyrics are in `LyricText`. | 200 |
| `Instrumental` | The song has no lyrics. | 200 |
| `NotFound` | At least one provider answered, none has the lyrics. | 404 |
| `AllProvidersFailed` | No provider could check the request (site unavailable, timeout or error): the lyrics may exist, retry later. | 503 |
| `EmptyRequest`, `EmptyArtistOrSong`, `EmptyUri` | The request is incomplete. | 400 |
| `ProviderNotAvailable` | The provider named in the request is not added to the client or is disabled. | 400 |
| `NoProviders`, `AllProvidersDisabled` | The client setup has no usable provider. | 500 |

`Code` and `ProviderAttemptOutcome` are written to JSON as strings (`"NotFound"`). New values may be added in minor versions, so handle unknown ones (`default` branch). In a `switch`, use the constants: `case SearchResultCode.Values.NotFound:`.

| `ProviderAttempts[i].Outcome` | Meaning |
|---|---|
| `Found`, `Instrumental` | The provider found the lyrics or an instrumental song. |
| `NotFound` | The provider answered and has no lyrics, also for a `404` page. |
| `Failed` | The site could not be loaded; `Message` has the HTTP status, for example `HTTP 503`. |
| `Timeout` | No answer within `ProviderTimeout`. |
| `Error` | The provider threw an exception; `Message` has its type and text. |
| `Canceled` | Stopped, because the search was canceled or another provider already gave the result (parallel search). |

For tests of your own code, `LyricsScraperModelFactory.SearchResult(...)` and `LyricsScraperModelFactory.ProviderAttempt(...)` create results, so a fake `ILyricsScraperClient` can return any of them.

Canceling `cancellationToken` throws `OperationCanceledException` right away.

The sync `SearchLyric` is obsolete ([LSNET0001](obsoletions/LSNET0001.md)) and will be removed in 4.0.0: it blocks the calling thread until the search ends and can starve the thread pool. Use `SearchLyricAsync`.

## Logging

```csharp
client.WithLogger(loggerFactory);
```

With dependency injection the client and providers get their loggers from the container.
