using LyricsScraperNET.Providers.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LyricsScraperNET.TestShared.Helpers
{
    public static class ExternalProviderTypeHelper
    {
        /// <summary>
        /// Provider types that have an implementation: not <see cref="ExternalProviderType.None"/> and not marked as obsolete.
        /// </summary>
        public static IReadOnlyList<ExternalProviderType> SupportedProviderTypes { get; } = GetProviderTypes(isRemoved: false);

        /// <summary>
        /// Provider types kept in the enum only for numeric compatibility after the provider was removed.
        /// </summary>
        public static IReadOnlyList<ExternalProviderType> RemovedProviderTypes { get; } = GetProviderTypes(isRemoved: true);

        private static IReadOnlyList<ExternalProviderType> GetProviderTypes(bool isRemoved)
            => Enum.GetValues<ExternalProviderType>()
                .Where(providerType => providerType != ExternalProviderType.None)
                .Where(providerType => IsObsolete(providerType) == isRemoved)
                .ToList();

        private static bool IsObsolete(ExternalProviderType providerType)
            => typeof(ExternalProviderType).GetField(providerType.ToString())!.IsDefined(typeof(ObsoleteAttribute), inherit: false);
    }
}
