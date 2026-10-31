using LyricsScraperNET.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;

namespace LyricsScraperNET.UnitTest.Common
{
    /// <summary>
    /// The rules of <see cref="Obsoletions"/> (docs/obsoletions/README.md): one nested class per id, unique ids,
    /// every [Obsolete] uses a known id and the shared URL, every id has a docs page.
    /// </summary>
    public class ObsoletionsTests
    {
        private const BindingFlags AllMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        [Fact]
        public void DiagnosticIds_OnePerNestedClass_UniqueAndInObsoletionRange()
        {
            // Act
            var ids = GetDiagnosticIds();

            // Assert
            Assert.Equal(new[] { "LSNET0001", "LSNET0002", "LSNET0003", "LSNET0004" }, ids.Values.OrderBy(id => id));
            Assert.All(ids.Values, id => Assert.Matches(new Regex("^LSNET0[0-9]{3}$"), id));
            Assert.Equal(ids.Count, ids.Values.Distinct().Count());
        }

        [Fact]
        public void ObsoleteAttributes_AllUseKnownIdAndSharedUrl()
        {
            // Arrange
            var knownIds = GetDiagnosticIds().Values.ToHashSet();

            // Act
            var attributes = GetObsoleteMembers().ToList();

            // Assert
            Assert.NotEmpty(attributes);
            Assert.All(attributes, entry =>
            {
                Assert.True(entry.Attribute.DiagnosticId != null && knownIds.Contains(entry.Attribute.DiagnosticId),
                    $"{entry.Member} has no known diagnostic id: '{entry.Attribute.DiagnosticId}'.");
                Assert.Equal(Obsoletions.SharedUrlFormat, entry.Attribute.UrlFormat);
                Assert.False(string.IsNullOrWhiteSpace(entry.Attribute.Message), $"{entry.Member} has no message.");
                Assert.False(entry.Attribute.IsError);
            });
            // Every id is used: an unused id means a forgotten attribute or a leftover class.
            Assert.Equal(knownIds.OrderBy(id => id), attributes.Select(entry => entry.Attribute.DiagnosticId).Distinct().OrderBy(id => id));
        }

        [Fact]
        public void DiagnosticIds_EachHasDocsPageAndIsListedInTheIndex()
        {
            // Arrange
            var docsDirectory = FindDocsDirectory();
            var index = File.ReadAllText(Path.Combine(docsDirectory, "README.md"));

            // Act & Assert
            Assert.All(GetDiagnosticIds().Values, id =>
            {
                var page = Path.Combine(docsDirectory, $"{id}.md");
                Assert.True(File.Exists(page), $"Missing docs page {page}.");
                Assert.StartsWith($"# {id}:", File.ReadAllText(page));
                Assert.Contains($"[{id}]({id}.md)", index);
            });
        }

        [Fact]
        public void SharedUrlFormat_PointsToTheDocsPageOfTheId()
        {
            // Act
            var url = string.Format(Obsoletions.SharedUrlFormat, "LSNET0001");

            // Assert
            Assert.Equal("https://github.com/skuill/LyricsScraperNET/blob/main/docs/obsoletions/LSNET0001.md", url);
        }

        private static Dictionary<string, string> GetDiagnosticIds()
            => typeof(Obsoletions).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)
                .ToDictionary(
                    type => type.Name,
                    type => (string)type.GetField("DiagnosticId", BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!);

        private static IEnumerable<(string Member, ObsoleteAttribute Attribute)> GetObsoleteMembers()
        {
            foreach (var type in typeof(Obsoletions).Assembly.GetTypes())
            {
                if (type.GetCustomAttribute<ObsoleteAttribute>() is { } typeAttribute)
                    yield return (type.FullName!, typeAttribute);

                foreach (var member in type.GetMembers(AllMembers))
                {
                    if (member.GetCustomAttribute<ObsoleteAttribute>(inherit: false) is { } memberAttribute)
                        yield return ($"{type.FullName}.{member.Name}", memberAttribute);
                }
            }
        }

        private static string FindDocsDirectory()
        {
            for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
            {
                var candidate = Path.Combine(directory.FullName, "docs", "obsoletions");
                if (Directory.Exists(candidate))
                    return candidate;
            }

            throw new DirectoryNotFoundException("docs/obsoletions not found above " + AppContext.BaseDirectory);
        }
    }
}
