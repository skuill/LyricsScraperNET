using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading;

namespace LyricsScraperNET.Configuration
{
    /// <summary>
    /// Logs a warning once per application if the configuration contains sections of providers that were removed from the library.
    /// Runs when <see cref="LyricScraperClientConfig"/> is created, so the warning appears on the first client resolved from DI.
    /// </summary>
    internal sealed class RemovedProvidersWarning : IPostConfigureOptions<LyricScraperClientConfig>
    {
        /// <summary>
        /// Configuration section names of providers removed in 3.0.0.
        /// </summary>
        internal static readonly IReadOnlyList<string> RemovedProviderSectionNames = new[]
        {
            "MusixmatchOptions",
            "LyricFindOptions"
        };

        private readonly ILogger? _logger;
        private readonly IReadOnlyCollection<string> _sectionNames;
        private int _isLogged;

        public RemovedProvidersWarning(ILoggerFactory? loggerFactory, IReadOnlyCollection<string> sectionNames)
        {
            _logger = loggerFactory?.CreateLogger<LyricsScraperClient>();
            _sectionNames = sectionNames;
        }

        public void PostConfigure(string? name, LyricScraperClientConfig options)
        {
            if (_logger == null || Interlocked.Exchange(ref _isLogged, 1) == 1)
                return;

            foreach (var sectionName in _sectionNames)
            {
                _logger.LogWarning(
                    "Configuration section '{ConfigurationSection}:{ProviderSection}' belongs to a provider that was removed in LyricsScraperNET 3.0.0. The section is ignored, please remove it from your configuration.",
                    LyricScraperClientConfig.ConfigurationSectionName,
                    sectionName);
            }
        }
    }
}
