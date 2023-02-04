using LyricsScraperNET.External.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsScraperNET.External.AZLyrics
{
    public sealed class AZLyricsOptions : IExternalServiceClientOptions
    {
        public bool Enabled { get; set; }

        public string ConfigurationSectionName => "AZLyricsOptions";
    }
}
