using System.IO;
using System.Text.Json;

namespace LyricsScraperNET.Test
{
    public static class Serializer
    {
        public static T Deseialize<T>(string[] paths)
        {
            var jsonString = File.ReadAllText(Path.GetFullPath(Path.Combine(paths)));
            return JsonSerializer.Deserialize<T>(jsonString);
        }
    }
}
