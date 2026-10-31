# HTTP

## Defaults

- All providers share one long-lived `HttpClient` that recycles its connections every 2 minutes (`PooledConnectionLifetime`) and decompresses responses, as recommended in the [HttpClient guidelines](https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient-guidelines). No client is created per request or per provider.
- Requests are sent with the User-Agent `LyricsScraperNET/1.0 (https://github.com/skuill/LyricsScraperNET)`, unless your client sets its own one.
- A failed request (network error, unsuccessful status code, timeout of the `HttpClient`) does not throw: the provider returns "not found" and the search continues with the next provider. Only cancellation by the caller throws `OperationCanceledException`.

## Own HttpClient

Use your own client, for example for a proxy, resilience handlers or your own User-Agent.

All providers, also the ones added later:

```csharp
ILyricsScraperClient lyricsScraperClient = new LyricsScraperClient()
    .WithHttpClient(sharedHttpClient)
    .WithAllProviders();
```

One provider:

```csharp
var geniusProvider = new GeniusProvider();
geniusProvider.WithHttpClient(geniusHttpClient);
lyricsScraperClient.AddProvider(geniusProvider);
```

The library does not dispose the client: keep it alive while the providers use it, and reuse one instance instead of creating a client per provider or request.

With dependency injection, configure the named client `ServiceCollectionExtensions.HttpClientName` instead, see [dependency-injection.md](dependency-injection.md#http-settings).

### Examples

Proxy:

```csharp
var httpClient = new HttpClient(new SocketsHttpHandler
{
    Proxy = new WebProxy("http://proxy.example:8080"),
    PooledConnectionLifetime = TimeSpan.FromMinutes(2),
});
```

Own User-Agent:

```csharp
httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MyApp/1.0 (+https://example.com)");
```

## Genius API key

With `GeniusOptions.ApiKey` (a "client access token" from [genius.com/api-clients](https://genius.com/api-clients)) the Genius provider searches through the official API. The key:

- is sent as `Authorization: Bearer` only to `api.genius.com`, never to the lyrics pages or other hosts;
- goes through the same `HttpClient` as all other requests;
- is read on every request, so it can be changed at runtime.

If the API request fails (for example an invalid key), the provider falls back to the public search of the site. Without a key only the public search is used.

## LRCLIB

LRCLIB asks clients to identify themselves: besides `User-Agent`, its requests always carry the header `X-User-Agent` with the library name, also when your client sets its own User-Agent. A `429 Too Many Requests` answer is retried once after `Retry-After` (at most 30 seconds).
