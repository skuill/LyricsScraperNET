using LyricsScraperNET.External.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsScraperNET.External.Genius
{
    public sealed class GeniusOptions : IExternalServiceClientOptions
    {
        public bool Enabled { get; set; }

        public string ConfigurationSectionName => "GeniusOptions";

        public string ApiKey { get; set; }
    }
}
