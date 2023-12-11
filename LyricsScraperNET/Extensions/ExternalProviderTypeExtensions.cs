using LyricsScraperNET.Providers.Models;

namespace LyricsScraperNET.Extensions
{
    internal static class ExternalProviderTypeExtensions
    {
        public static bool IsNoneProviderType(this ExternalProviderType providerType)
            => providerType == ExternalProviderType.None;
    }
}
