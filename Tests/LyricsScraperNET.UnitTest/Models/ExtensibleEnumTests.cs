using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Xunit;

namespace LyricsScraperNET.UnitTest.Models
{
    /// <summary>
    /// Behavior shared by all extensible enums (string structs with well-known values).
    /// </summary>
    public abstract class ExtensibleEnumTests<T>
        where T : struct, IEquatable<T>
    {
        /// <summary>
        /// The public constructor of the tested struct.
        /// </summary>
        protected abstract T Create(string value);

        [Fact]
        public void Default_IsNone()
        {
            // Act
            var value = default(T);

            // Assert
            Assert.Equal("None", value.ToString());
            Assert.Equal(Create("None"), value);
            Assert.Equal("None", value.ToString());
        }

        [Fact]
        public void Equals_IgnoresCase()
        {
            // Arrange
            var lower = Create("notfound");
            var upper = Create("NotFound");

            // Act & Assert
            Assert.True(lower.Equals(upper));
            Assert.Equal(lower.GetHashCode(), upper.GetHashCode());
            Assert.False(lower.Equals(Create("Found")));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Constructor_NullOrEmpty_Throws(string value)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => Create(value));
        }

        [Fact]
        public void StaticValues_MatchTheValuesConstantsByName()
        {
            // Arrange
            var staticValues = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(property => property.PropertyType == typeof(T))
                .ToDictionary(property => property.Name, property => ((T)property.GetValue(null)!).ToString());
            var constants = typeof(T).GetNestedType("Values")!.GetFields(BindingFlags.Public | BindingFlags.Static)
                .ToDictionary(field => field.Name, field => (string)field.GetRawConstantValue()!);

            // Assert
            Assert.NotEmpty(staticValues);
            Assert.Equal(constants.OrderBy(x => x.Key), staticValues.OrderBy(x => x.Key));
            Assert.All(staticValues, pair => Assert.Equal(pair.Key, pair.Value));
        }

        [Fact]
        public void Json_WritesStringAndReadsItBack()
        {
            // Arrange
            var known = Create("NotFound");
            var unknown = Create("AddedInAFutureVersion");

            // Act
            var knownJson = JsonSerializer.Serialize(known);
            var unknownJson = JsonSerializer.Serialize(unknown);
            var defaultJson = JsonSerializer.Serialize(default(T));

            // Assert
            Assert.Equal("\"NotFound\"", knownJson);
            Assert.Equal("\"AddedInAFutureVersion\"", unknownJson);
            Assert.Equal("\"None\"", defaultJson);
            Assert.Equal(known, JsonSerializer.Deserialize<T>(knownJson));
            Assert.Equal(unknown, JsonSerializer.Deserialize<T>(unknownJson));
            Assert.Equal(default(T), JsonSerializer.Deserialize<T>("\"\""));
        }

        [Theory]
        [InlineData("1")]
        [InlineData("{}")]
        [InlineData("null")]
        public void Json_NotAString_Throws(string json)
        {
            // Act & Assert
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<T>(json));
        }
    }
}
