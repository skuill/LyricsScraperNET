using System;
using System.IO;
using System.Text.Json;

namespace LyricsScraperNET.TestShared.Utils
{
    public static class Serializer
    {
        public static T Deseialize<T>(string path)
        {
            var jsonString = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, path));
            return JsonSerializer.Deserialize<T>(jsonString);
        }
    }
}
