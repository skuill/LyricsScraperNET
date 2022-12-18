using System.IO;
using System.Text.Json;

namespace LyricsScraper.Test
{
    public static class Serializer
    {
        public static T Deseialize<T>(string fileName)
        {
            var jsonString = File.ReadAllText(fileName);
            return JsonSerializer.Deserialize<T>(jsonString);
        }
    }
}
