using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LyricsScraperNET.TestShared.Helpers
{
    public static class PathHelper
    {
        public static string GetFullPathInCurrentDirectory(string path)
        {
            IEnumerable<string> paths = new List<string> { Environment.CurrentDirectory }.Concat(path.Split(new char[] { '\\', '/' }));
            return Path.Combine(paths.ToArray());
        }
    }
}
