using LyricsScraperNET.External.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsScraperNET.Configuration
{
    public sealed class LyricScraperClientConfig : ILyricScraperClientConfig
    {
        public IEnumerable<IExternalServiceClientOptions> ClientOptions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
