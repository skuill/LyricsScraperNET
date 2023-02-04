using LyricsScraperNET.External.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsScraperNET.Configuration
{
    public interface ILyricScraperClientConfig
    {
        IEnumerable<IExternalServiceClientOptions> ClientOptions { get; set; }
    }
}
