using System.Text.Json.Serialization;

namespace LyricsScraperNET.Providers.Lrclib
{
    internal sealed class LrclibTrackResponse
    {
        [JsonPropertyName("instrumental")]
        public bool Instrumental { get; set; }

        [JsonPropertyName("plainLyrics")]
        public string? PlainLyrics { get; set; }

        [JsonPropertyName("code")]
        public int? Code { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
