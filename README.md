# LyricsScraperNET

[![CodeFactor](https://www.codefactor.io/repository/github/skuill/lyricsscrapernet/badge)](https://www.codefactor.io/repository/github/skuill/lyricsscrapernet)
[![CI/CD LyricsScraperNET](https://github.com/skuill/LyricsScraperNET/actions/workflows/cicd.yaml/badge.svg)](https://github.com/skuill/LyricsScraperNET/actions/workflows/cicd.yaml)
[![NuGet](https://img.shields.io/nuget/vpre/LyricsScraperNET?label=NuGet)](https://www.nuget.org/packages/LyricsScraperNET/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/LyricsScraperNET?label=Downloads)](https://www.nuget.org/packages/LyricsScraperNET/)
[![License](https://img.shields.io/github/license/skuill/LyricsScraperNET)](./LICENSE)

**LyricsScraperNET** is a versatile .NET library that provides an API for searching song lyrics from various external providers. 

---

## 🌟 Features

- **Multi-framework support**: Compatible with `.NET Standard 2.x`, `.NET 5`, `.NET 6`, `.NET 7`, `.NET 8`.
- **Integrated logging**: Effortless debugging and tracking.
- **Modular architecture**: Highly testable and customizable.
- **Flexible configuration**: Multiple ways to configure the library.
- **NuGet support**: Easy installation from [NuGet](https://www.nuget.org/packages/LyricsScraperNET/).

---

## 🎤 Supported Lyrics Providers

The library currently supports the following providers:

- [AZLyrics](https://www.azlyrics.com/)
- [Genius](https://genius.com/)
- [MusixMatch](https://www.musixmatch.com/)
- [SongLyrics](https://www.songlyrics.com/)
- [LyricFind](https://www.lyricfind.com/)
- [Lyrics](https://www.lyrics.com/) (**Coming soon** 🚧)

---

## 📋 Getting Started

Here’s a simple example to demonstrate how to initialize and use the library with all available providers:

```csharp
using LyricsScraperNET;
using LyricsScraperNET.Models.Requests;

class Program
{
    static async Task Main()
    {
        // Create an instance of LyricScraperClient with all providers
        ILyricsScraperClient lyricsScraperClient = new LyricsScraperClient()
            .WithAllProviders();

        var searchRequest = new ArtistAndSongSearchRequest(
            artist: "Metallica", 
            song: "Nothing Else Matters"
        );

        var searchResult = lyricsScraperClient.SearchLyric(searchRequest);

        if (!searchResult.IsEmpty())
        {
            Console.WriteLine(searchResult.LyricText);
        }
    }
}
```

For more advanced usage, you can set up the `LyricScraperClient` via dependency injection (`IServiceCollection`) with configurations stored in `IConfiguration`.

Explore additional examples in the [LyricsScraperNET.Client](LyricsScraperNET.Client/Program.cs) project.

---

## 🛠️ Installation

Install the package from NuGet:

```sh
dotnet add package LyricsScraperNET
```

---

## 🖋️ Contributing and Feedback

Contributions are welcome! 

- Fork the repository and create a [Pull Request](https://github.com/skuill/LyricScraperNET/pulls).
- Report issues or propose features via the [Issues](https://github.com/skuill/LyricScraperNET/issues/new) tab.

For direct feedback, feel free to reach out on [Telegram](https://t.me/skuill).

---

## 💖 Support

If you find this project helpful, consider supporting it:

[![Buy Me A Coffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/skuill)  
[![Tinkoff Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.tinkoff.ru/cf/3MNYeRds3s)

---

## 📜 License

This project is licensed under the [MIT License](./LICENSE).
