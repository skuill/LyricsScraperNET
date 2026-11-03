using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LyricsScraperNET.Extensions
{
    /// <summary>
    /// Compares an artist or song name from a site with the one from the search request.
    /// </summary>
    internal static class TextMatchingExtensions
    {
        // Straight, typographic and modifier letter apostrophes, backtick.
        private const string Apostrophes = "'’‘`ʼ";

        /// <summary>
        /// Compares letters and digits only, ignoring case and diacritics. Examples:
        /// "Beyonce" = "Beyoncé", "Jay-Z" = "JAŸ-Z", "Guns N Roses" = "Guns N’ Roses",
        /// "Harder, Better, Faster, Stronger" = "Harder Better Faster Stronger", "Rock &amp; Roll" = "Rock and Roll",
        /// "What Happens If I Cant Check My MySpace" = "What Happens If I Can’t Check My MySpace?".
        /// Texts without letters and digits (for example the artist "!!!") are compared as they are.
        /// </summary>
        public static bool IsSameText(this string? actual, string expected)
        {
            if (actual == null)
                return false;

            var normalizedActual = NormalizeText(actual);
            var normalizedExpected = NormalizeText(expected);
            if (normalizedActual.Length == 0 || normalizedExpected.Length == 0)
                return string.Equals(actual.Trim(), expected.Trim(), StringComparison.OrdinalIgnoreCase);

            return string.Equals(normalizedActual, normalizedExpected, StringComparison.Ordinal);
        }

        private static string NormalizeText(string text)
        {
            var builder = new StringBuilder(text.Length);
            foreach (var c in text.Replace("&", " and ").Normalize(NormalizationForm.FormD))
            {
                if (char.IsLetterOrDigit(c))
                    builder.Append(char.ToLowerInvariant(c));
                // Apostrophes join the word parts: "Don’t" = "Dont". Diacritics are dropped: "é" = "e".
                else if (Apostrophes.Contains(c)
                    || CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                    continue;
                else
                    builder.Append(' ');
            }

            // Genius writes parts as "Pt.": "Dr. Shavargo, Pt. 3" = "Dr. Shavargo Part 3".
            return string.Join(" ", builder.ToString()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(word => word == "pt" ? "part" : word));
        }
    }
}
