# Dependency injection

`AddLyricScraperClientService` registers the client and the providers from the `LyricScraperClient` configuration section (see [configuration.md](configuration.md#configuration-file)).

```csharp
using LyricsScraperNET;
using LyricsScraperNET.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLyricScraperClientService(builder.Configuration);

var app = builder.Build();

app.MapGet("/lyrics", async (string artist, string song, ILyricsScraperClient client, CancellationToken cancellationToken) =>
{
    var result = await client.SearchLyricAsync(new ArtistAndSongSearchRequest(artist, song), cancellationToken);
    return result.IsEmpty() && !result.Instrumental ? Results.NotFound() : Results.Ok(result.LyricText);
});

app.Run();
```

A console app with the generic host is in [LyricsScraperNET.Client/Program.cs](../LyricsScraperNET.Client/Program.cs).

## What is registered

| Service | Lifetime | Notes |
|---|---|---|
| `ILyricsScraperClient` | Scoped | Always registered. |
| `ILyricScraperClientConfig` | Scoped | Only if the `LyricScraperClient` section exists. Read through `IOptionsSnapshot`, so a changed configuration file is picked up by new scopes. |
| `IExternalProvider` (one per provider) | Scoped | Only for providers whose section exists, for example `GeniusOptions`. A provider without a section is not used. |
| Named `HttpClient` `ServiceCollectionExtensions.HttpClientName` | `IHttpClientFactory` | Every scoped provider gets a new client of the factory. |

## HTTP settings

The providers send their requests through the named client `ServiceCollectionExtensions.HttpClientName` (`"LyricsScraperNET"`). Configure it like any named client:

```csharp
services.AddLyricScraperClientService(configuration);
services.AddHttpClient(ServiceCollectionExtensions.HttpClientName)
    .ConfigureHttpClient(client => client.DefaultRequestHeaders.UserAgent.ParseAdd("MyApp/1.0"))
    .AddStandardResilienceHandler(); // Microsoft.Extensions.Http.Resilience
```

More options (proxy, User-Agent, Genius API key) are in [http.md](http.md).
