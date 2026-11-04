# Providers

| Provider | `ExternalProviderType` | Search by artist and song | Search by URI | Instrumental detection |
|---|---|---|---|---|
| [LRCLIB](https://lrclib.net/) | `Lrclib` (8) | Official API (`/api/get`) | Yes | Yes (API flag) |
| [AZLyrics](https://www.azlyrics.com/) | `AZLyrics` (1) | Page URL built from artist and song | Yes | No |
| [Genius](https://genius.com/) | `Genius` (2) | Site search or official API | Yes | Yes |
| [SongLyrics](https://www.songlyrics.com/) | `SongLyrics` (4) | Page URL built from artist and song | Yes | Yes |
| [LyricsFreak](https://www.lyricsfreak.com/) | `LyricsFreak` (7) | Artist page, then the song link on it | Yes | No |
| [kpoplyrics](https://www.kpoplyrics.net/) | `KPopLyrics` (6) | Site search, then the post with the same artist and song | Yes | No |

The numeric values of `ExternalProviderType` are stable: new providers get new values, removed providers keep theirs as `[Obsolete]` members.

## Genius

- **Search:** without an API key the provider uses the public search of the site, with `GeniusOptions.ApiKey` the official API (details in [http.md](http.md#genius-api-key)).
- **Choosing a hit:** lyrics are returned only for a hit whose primary artist and title match the request. The first hit is often another song (a live version, a remix, a translation page or another song of the artist), so it is never taken blindly. If no hit matches, the result is `NoDataFound`.
- **Matching rules** (also used by LyricsFreak and KPopLyrics): case, diacritics and punctuation are ignored, apostrophes are removed, `&` equals `and`, `Pt.` equals `Part`. Examples:

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

- Searches the exact artist and track name through the official API; matching is done by LRCLIB (case, diacritics, punctuation and apostrophes are ignored by the server).
- `&` and `and`: the server turns `&` into a space, so `Rock & Roll` does not find `Rock And Roll`, and the database keeps either spelling. If the track is not found and the request has `&` (or the word `and`), the provider tries the other spelling once.
- `503` (server busy) and `429` (rate limit) are retried once after `Retry-After`.
- Returns plain lyrics (`plainLyrics`), not synced ones.
- Some recordings are marked as instrumental by LRCLIB even if other providers have lyrics for the song. With the default priorities LRCLIB is asked first, so such a result ends the search.

## AZLyrics, SongLyrics

These providers build the page URL from the artist and the song (lowercase, without most punctuation), so the request must be written close to how the site names the song.

- They replace letters with diacritics by their base letters like the sites do: `Motörhead` is `motorhead`, `Zé Ramalho` is `ze-ramalho` on SongLyrics. Letters without a base letter (`ø`, `ß`) and other scripts are not converted.
- AZLyrics drops `&` like the site (`Simon & Garfunkel` is `simongarfunkel`), so `Simon and Garfunkel` finds nothing there.
- Aliases and other title spellings are not handled: AZLyrics names Beyoncé `beyonceknowles`.

## KPopLyrics

Searches the posts of the site (the WordPress search API) and loads the post titled `{artist} – {song} Lyrics` (two requests per search). Page URLs are not built from the names: the site redirects a guessed URL to the closest post, which can be another song (`BTS` / `Dynamite` used to return the Holiday Remix, [#56](https://github.com/skuill/LyricsScraperNET/issues/56)).

- **Names:** post titles use Latin names only (`JENNIE – Mantra Lyrics`). A part in parentheses without Latin letters is ignored, so `JENNIE (제니)` / `Mantra` finds the post too.
- **Matching:** the same rules as Genius (case, diacritics, punctuation and apostrophes ignored, `&` equals `and`). Only the whole title matches: `Dynamite` does not find `Dynamite (Holiday Remix)`.
- Names without Latin letters or digits (`방탄소년단`) return `NoDataFound` without requests.

## LyricsFreak

Loads the artist page first and looks for the song link on it (two requests per search).

- **Artist page:** the site mostly uses base letters (`Beyoncé` is `/b/beyonce/`), but some artists keep the name with these letters dropped (`Zé Ramalho` is `/z/z+ramalho/`). The provider tries the base-letter page first and the other one if the first is missing (one more request).
- **Song link:** compared with the same rules as Genius (case, diacritics, punctuation and apostrophes ignored, `&` equals `and`), so `Sweet Child O Mine` finds `Sweet Child O' Mine`. Only the whole title matches: `Halo` does not find `Halo (Live)`.
- Artist names without Latin letters or digits (for example Cyrillic or Korean) return `NoDataFound` without requests: the site has no pages for them.
- Some songs are listed on the artist page without a lyrics page; they return `NoDataFound`.
- **Rate limit:** after a quick series of requests the site answers `429`; the provider waits 2 seconds and retries once (details in [http.md](http.md#lyricsfreak)).

## Removed providers

| Provider | Removed in | Reason | Issue |
|---|---|---|---|
| [MusixMatch](https://www.musixmatch.com/) | 3.0.0 | The site serves decoy lyrics to non-authorized clients, so the provider returned wrong text for any query. | [#54](https://github.com/skuill/LyricsScraperNET/issues/54) |
| [LyricFind](https://www.lyricfind.com/) | 3.0.0 | Lyrics pages are behind a JavaScript bot challenge and cannot be loaded with plain HTTP requests. | [#29](https://github.com/skuill/LyricsScraperNET/issues/29) |

`ExternalProviderType.Musixmatch` (3) and `ExternalProviderType.LyricFind` (5) are kept as `[Obsolete]` members; `WithMusixmatch()` and `WithLyricFind()` are `[Obsolete]` and do nothing.
