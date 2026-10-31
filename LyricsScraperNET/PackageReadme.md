# LyricsScraperNET

A .NET library that searches song lyrics on several lyrics sites with one API. Targets .NET 8, 9 and 10.

**Documentation, providers, configuration and the upgrade guide: [github.com/skuill/LyricsScraperNET](https://github.com/skuill/LyricsScraperNET#readme)**

## Quick start

```csharp
using LyricsScraperNET;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;

ILyricsScraperClient client = new LyricsScraperClient()
    .WithAllProviders();

var result = await client.SearchLyricAsync(
    new ArtistAndSongSearchRequest("John Newton", "Amazing Grace"));

if (result.Code == SearchResultCode.Found)
    Console.WriteLine(result.LyricText);
```

## Responsible use

LyricsScraperNET does not host, store or redistribute lyrics; it requests them from the providers on behalf of your application. Lyrics are protected by copyright: follow the terms of use of each provider and the copyright law that applies to you.
