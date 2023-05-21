# LyricsScraperNET

[![CI/CD LyricsScraperNET](https://github.com/skuill/LyricsScraperNET/actions/workflows/cicd.yaml/badge.svg)](https://github.com/skuill/LyricsScraperNET/actions/workflows/cicd.yaml)
[![LirycsScraperNET](https://img.shields.io/nuget/vpre/LyricsScraperNET?label=LyricsScraperNET)](https://www.nuget.org/packages/LyricsScraperNET/)
[![License](https://img.shields.io/github/license/skuill/LyricsScraperNET)](./LICENSE)

LyricsScraperNET is a library for .NET that provides an API to search for lyrics of a song from the web. 

## Features

* ✅ Supports multiple frameworks `.NET Standard 2.X`, `.NET 5`, `.NET 6`, `.NET 7`
* ✅ Logging supported.
* ✅ Modular structure, for easy unit testing.
* ✅ Multiple ways how to configure.
* ✅ Easily installed and used from the nuget repository.

## Supported external lyrics providers

- AZLyrics
- Genius
- MusixMatch (**IN PROGRESS. [Issue #2](/../../issues/2)**)
- LyricsWikia (**TODO**)
- SongLyrics (**TODO**)

## Example

```csharp
using LyricsScraperNET;
using LyricsScraperNET.Models.Requests;

class Program
{
    static async Task Main()
    {
        // Create instance of LyricScraperClient with Genius and AZLyrics providers
        ILyricsScraperClient lyricsScraperClient 
            = new LyricsScraperClient()
                .WithAZLyrics()
                .WithGenius()
                .WithMusixmatch();

        var searchRequest = new ArtistAndSongSearchRequest(artist: "Metallica", song: "Nothing Else Matters");

        var searchResult = lyricsScraperClient.SearchLyric(searchRequest);

        if (!searchResult.IsEmpty())
            Console.WriteLine(searchResult.LyricText);
    }
}
```
More examples can be found in example project: [LyricsScraperNET.Client](LyricsScraperNET.Client/Program.cs)

## Contributing and Feedback

Feel free to send me feedback on [Telegram](https://t.me/skuill).
You are more than welcome to contribute to this project. Fork and make a Pull Request, or [create an Issue](https://github.com/skuill/LyricScraperNET/issues/new) if you see any problem or want to propose a feature.

## Support
If you want to support this project or my work in general, you can donate via the link below. 

This will always be optional! Thank you! 😉

 * [!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/skuill)
 * [!["Tinkoff Donate Button"](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.tinkoff.ru/cf/3MNYeRds3s)
