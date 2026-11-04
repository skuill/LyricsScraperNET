using System.Text.Json.Serialization;

namespace LyricsScraperNET.Providers.KPopLyrics
{
    /// <summary>
    /// A post from the site search, see <see cref="KPopLyricsUriConverter.GetLyricUri"/>.
    /// </summary>
    internal sealed class KPopLyricsPost
    {
        [JsonPropertyName("link")]
        public string? Link { get; set; }

        [JsonPropertyName("title")]
        public KPopLyricsPostTitle? Title { get; set; }
    }

    internal sealed class KPopLyricsPostTitle
    {
        // HTML-encoded: "BTS &#8211; Dynamite Lyrics".
        [JsonPropertyName("rendered")]
        public string? Rendered { get; set; }
    }
}
