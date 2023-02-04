using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsScraperNET.External.Abstract
{
    public interface IExternalServiceClientOptions
    {
        bool Enabled { get; set; }

        string ConfigurationSectionName { get; }
    }
}
