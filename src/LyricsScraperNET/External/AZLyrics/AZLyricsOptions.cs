using LyricsScraperNET.External.Abstract;
using LyricsScraperNET.External.Models;

namespace LyricsScraperNET.External.AZLyrics
{
    public sealed class AZLyricsOptions : IExternalServiceClientOptions
    {
        public bool Enabled { get; set; }

        public ExternalServiceType ExternalServiceType => ExternalServiceType.AZLyrics;

        public const string ConfigurationSectionName = "AZLyricsOptions";

        public override bool Equals(object? obj)
        {
            return obj is AZLyricsOptions options &&
                   ExternalServiceType == options.ExternalServiceType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ExternalServiceType);
        }
    }
}
