using LyricsScraperNET.External.Abstract;
using LyricsScraperNET.External.Models;

namespace LyricsScraperNET.External.Genius
{
    public sealed class GeniusOptions : IExternalServiceClientOptions
    {
        public bool Enabled { get; set; }

        public string ApiKey { get; set; }

        public const string ConfigurationSectionName = "GeniusOptions";

        public ExternalServiceType ExternalServiceType => ExternalServiceType.Genius;

        public override bool Equals(object? obj)
        {
            return obj is GeniusOptions options &&
                   ApiKey == options.ApiKey &&
                   ExternalServiceType == options.ExternalServiceType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ApiKey, ExternalServiceType);
        }
    }
}
