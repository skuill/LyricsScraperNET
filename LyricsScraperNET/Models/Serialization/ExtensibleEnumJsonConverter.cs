using LyricsScraperNET.Models.Abstract;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LyricsScraperNET.Models.Serialization
{
    /// <summary>
    /// Writes an extensible enum as its string value and reads it back.
    /// </summary>
    internal sealed class ExtensibleEnumJsonConverter<T> : JsonConverter<T>
        where T : struct, IExtensibleEnum<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Expected a string for {typeof(T).Name}, got {reader.TokenType}.");

            var value = reader.GetString();
            return string.IsNullOrEmpty(value) ? default : T.Create(value);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.Value);
    }
}
