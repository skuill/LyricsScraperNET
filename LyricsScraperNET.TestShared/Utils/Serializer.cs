using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace LyricsScraperNET.TestShared.Utils
{
    public static class Serializer
    {
        public static T Deseialize<T>(string path)
        {
            IEnumerable<string> paths = new List<string> { Environment.CurrentDirectory }.Concat(path.Split(new char[] { '\\', '/' }));
            var jsonString = File.ReadAllText(Path.Combine(paths.ToArray()));
            return JsonSerializer.Deserialize<T>(jsonString);
        }
    }
}
