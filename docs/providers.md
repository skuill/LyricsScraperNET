# Providers

| Provider | `ExternalProviderType` | Search by artist and song | Search by URI | Instrumental detection |
|---|---|---|---|---|
| [LRCLIB](https://lrclib.net/) | `Lrclib` (8) | Official API (`/api/get`) | Yes | Yes (API flag) |
| [AZLyrics](https://www.azlyrics.com/) | `AZLyrics` (1) | Page URL built from artist and song | Yes | No |
| [Genius](https://genius.com/) | `Genius` (2) | Site search or official API | Yes | Yes |
| [SongLyrics](https://www.songlyrics.com/) | `SongLyrics` (4) | Page URL built from artist and song | Yes | Yes |
| [LyricsFreak](https://www.lyricsfreak.com/) | `LyricsFreak` (7) | Artist page, then the song link on it | Yes | No |
| [kpoplyrics](https://www.kpoplyrics.net/) | `KPopLyrics` (6) | Page URL built from artist and song | Yes | No |

The numeric values of `ExternalProviderType` are stable: new providers get new values, removed providers keep theirs as `[Obsolete]` members.

## Genius

- **Search:** without an API key the provider uses the public search of the site, with `GeniusOptions.ApiKey` the official API (details in [http.md](http.md#genius-api-key)).
- **Choosing a hit:** lyrics are returned only for a hit whose primary artist and title match the request. The first hit is often another song (a live version, a remix, a translation page or another song of the artist), so it is never taken blindly. If no hit matches, the result is `NoDataFound`.
- **Matching rules:** case, diacritics and punctuation are ignored, apostrophes are removed, `&` equals `and`, `Pt.` equals `Part`. Examples:

  | Request | Genius |
  |---|---|
  | `Beyonce` | `Beyoncé` |
  | `Jay-Z` | `JAŸ-Z` |
  | `Guns N Roses` / `Sweet Child O Mine` | `Guns N’ Roses` / `Sweet Child O’ Mine` |
  | `Attack Attack` / `What Happens If I Cant Check My MySpace When We Get There` | `Attack Attack!` / `What Happens If I Can’t Check My MySpace When We Get There?` |
  | `Daft Punk` / `Harder, Better, Faster, Stronger` | `Daft Punk` / `Harder Better Faster Stronger` |
  | `AC/DC` / `Rock & Roll Ain't Noise Pollution` | `AC/DC` / `Rock and Roll Ain’t Noise Pollution` |
  | `Attack Attack!` / `Dr. Shavargo Part 3` | `Attack Attack!` / `Dr. Shavargo, Pt. 3` |

  Not matched on purpose: other versions of a song (`Halo` does not match `Halo (Live)`), aliases (`Ye` is not `Kanye West`), and an incomplete version name (`All Too Well (10 Minute Version)` does not match `All Too Well (10 Minute Version) (Taylor’s Version) [From The Vault]`). Search by URI for such cases.
- **Instrumental songs** (for example Rush "YYZ") return `Success` with `Instrumental = true` and empty `LyricText`.

## LRCLIB

- Searches the exact artist and track name through the official API; matching is done by LRCLIB.
- Returns plain lyrics (`plainLyrics`), not synced ones.
- Some recordings are marked as instrumental by LRCLIB even if other providers have lyrics for the song. With the default priorities LRCLIB is asked first, so such a result ends the search.

## AZLyrics, SongLyrics, KPopLyrics

These providers build the page URL from the artist and the song (lowercase, without most punctuation), so the request must be written close to how the site names the song. Diacritics are not handled yet: `Beyoncé` finds nothing on AZLyrics, `Beyonce` does.

KPopLyrics: the site answers a missing page with a redirect to the closest post it can guess, which can be another version of the song. Details in [#56](https://github.com/skuill/LyricsScraperNET/issues/56).

## LyricsFreak

Loads the artist page first and looks for the song link on it (two requests per search). Artist names without Latin letters or digits are not supported yet ([#62](https://github.com/skuill/LyricsScraperNET/issues/62)).

## Removed providers

| Provider | Removed in | Reason | Issue |
|---|---|---|---|
| [MusixMatch](https://www.musixmatch.com/) | 3.0.0 | The site serves decoy lyrics to non-authorized clients, so the provider returned wrong text for any query. | [#54](https://github.com/skuill/LyricsScraperNET/issues/54) |
| [LyricFind](https://www.lyricfind.com/) | 3.0.0 | Lyrics pages are behind a JavaScript bot challenge and cannot be loaded with plain HTTP requests. | [#29](https://github.com/skuill/LyricsScraperNET/issues/29) |

`ExternalProviderType.Musixmatch` (3) and `ExternalProviderType.LyricFind` (5) are kept as `[Obsolete]` members; `WithMusixmatch()` and `WithLyricFind()` are `[Obsolete]` and do nothing.
