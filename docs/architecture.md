# Architecture

## Components

```
ILyricsScraperClient (LyricsScraperClient)
 ├─ RequestValidator          checks the request before the search
 ├─ ProviderService           thread-safe list of providers (copy-on-write)
 └─ IExternalProvider         one per site (ExternalProviderBase)
     ├─ IExternalProviderOptions   Enabled, SearchPriority, provider settings
     ├─ IExternalUriConverter      artist and song -> page or API URI
     ├─ IWebClient                 loads the URI (NetHttpClient, LrclibHttpClient)
     │   └─ HttpClient             SharedHttpClient, your own client or IHttpClientFactory
     └─ IExternalProviderLyricParser   cleans the text found on the page
```

| Folder | Content |
|---|---|
| `LyricsScraperNET/` | `ILyricsScraperClient` and `LyricsScraperClient`: entry point and search orchestration. |
| `Configuration/` | `LyricScraperClientConfig`, dependency injection (`ServiceCollectionExtensions`), the warning for removed providers. |
| `Extensions/` | Fluent setup (`WithAllProviders()`, `WithGenius()`, ...), helpers. |
| `Models/` | Requests (`ArtistAndSongSearchRequest`, `UriSearchRequest`) and `SearchResult`. |
| `Network/` | `IWebClient`, `NetHttpClient`, `SharedHttpClient`. |
| `Providers/<Name>/` | One folder per provider: `<Name>Provider`, `<Name>Options`, `<Name>UriConverter`, `<Name>Parser`. |
| `Providers/Abstract/` | `ExternalProviderBase` and the provider interfaces. |
| `Validations/` | `RequestValidator`. |

## Search flow

1. `LyricsScraperClient.SearchLyricAsync` validates the request (`BadRequest` for an empty request, empty fields or a provider that is not configured).
2. It takes a snapshot of the enabled providers ordered by `SearchPriority`, and of the settings (parallel mode, timeout).
3. Each provider runs in `SearchWithProvider` with its own token: the caller token linked with `ProviderTimeout`. A timeout or an exception of a provider is logged and counts as "not found".
4. A result counts as found if it has lyrics or is instrumental. Sequential mode stops at the first found result; parallel mode returns the found result with the highest priority once no provider with a higher priority is still running (see [configuration.md](configuration.md#search-order)).
5. If nothing is found, the result is `NoDataFound`.

Inside a provider:
1. The URI converter builds the page URI, or the provider searches first (Genius: site or API search, LyricsFreak: artist page).
2. The web client loads the page. Following the [web client contract](migration-2.x-to-3.0.md#web-client-contract), a failed request returns an empty string and never throws, except for caller cancellation.
3. The provider extracts the lyrics with HtmlAgilityPack (or JSON for LRCLIB and the Genius search) and the parser cleans the text.

## HTTP

One static `HttpClient` (`SharedHttpClient`) is shared by all providers unless the consumer sets its own client (`WithHttpClient`) or uses dependency injection (named client from `IHttpClientFactory`). Details in [http.md](http.md).

## Tests

| Project | Content |
|---|---|
| `Tests/LyricsScraperNET.UnitTest` | No network. Providers run over a fake `IWebClient` or a real `HttpClient` over a fake `HttpMessageHandler`. Test pages are small synthetic HTML files with placeholder text. |
| `Tests/LyricsScraperNET.IntegrationTest` | Live sites, not run in CI. Expected lyrics are public domain songs. |
| `Tests/LyricsScraperNET.TestShared` | Test data model and helpers. |

Real lyrics and saved site pages are not committed as test data ([#69](https://github.com/skuill/LyricsScraperNET/issues/69)).

## Adding a provider

1. Copy the folder of a similar provider (for example `Providers/SongLyrics/`) and rename the classes.
2. Add a member to `ExternalProviderType` with a new explicit value. Never change the existing values: consumers map the enum by number.
3. Add the default priority to `Common/Constants.cs`, the options to `LyricScraperClientConfig`, the registration to `ServiceCollectionExtensions` and `With<Name>()` to `LyricsScraperClientExtensions` (also in `WithAllProviders()`).
4. Add unit tests with synthetic pages and integration tests.
5. Follow the "Responsible Use" section of the [README](../README.md): no bypassing of bot protection or other access restrictions.
