using LyricsScraperNET.Providers.Abstract;

namespace LyricsScraperNET.Extensions
{
    internal static class ExternalProviderOptionsExtensions
    {
        public static bool TryGetApiKeyFromOptions(this IExternalProviderOptions options, out string apiKey)
        {
            apiKey = string.Empty;
            var optionsWithApiKey = options as IExternalProviderOptionsWithApiKey;

            if (optionsWithApiKey == null || string.IsNullOrWhiteSpace(optionsWithApiKey.ApiKey))
            {
                return false;
            }
            apiKey = optionsWithApiKey.ApiKey;
            return true;
        }
    }
}
