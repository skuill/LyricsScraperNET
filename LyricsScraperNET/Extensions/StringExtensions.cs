using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace LyricsScraperNET.Extensions
{
    internal static class StringExtensions
    {
        private static readonly string[] ARTICLES = { "a", "on", "the" };
        private static readonly char[] EXCEPTION_SYMBOLS = { '!' };

        public static string RemoveHtmlTags(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            int idx = text.IndexOf('<');

            while (idx >= 0)
            {
                var endIdx = text.IndexOf('>', idx + 1);
                if (endIdx < idx)
                {
                    break;
                }
                text = text.Remove(idx, endIdx - idx + 1);
                idx = text.IndexOf('<', idx);
            }

            return text;
        }

        public static string StripRedundantChars(this string input, bool removeArticle = false)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var result = input.ToLowerInvariant().Trim();

            if (removeArticle)
                foreach (var article in ARTICLES)
                {
                    if (result.StartsWith($"{article} ", StringComparison.InvariantCultureIgnoreCase))
                    {
                        result = result.Remove(0, article.Length);
                        break;
                    }
                }

            result = new string(result.Where(c => char.IsLetterOrDigit(c)).ToArray());

            return result;
        }

        public static string СonvertToDashedFormat(this string input, bool useExceptionSymbols = true, bool removeProhibitedSymbols = false)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var result = input.ToLowerInvariant().Trim();

            if (removeProhibitedSymbols)
                result = new string(result.Where(x => char.IsLetterOrDigit(x) || char.IsWhiteSpace(x) || x == '-' || x == '.').ToArray());

            result = Regex.Replace(new string(result.Select(x =>
            {
                return (char.IsLetterOrDigit(x) || (useExceptionSymbols && EXCEPTION_SYMBOLS.Contains(x)))
                    ? x
                    : '-';
            }).ToArray()), "-+", "-").Trim('-');

            return result;
        }

        public static string CreateCombinedUrlSlug(string artist, string songTitle)
        {
            artist = Regex.Replace(artist, @"\([^a-zA-Z0-9\s]*\)", "").Trim();
            songTitle = Regex.Replace(songTitle, @"\([^a-zA-Z0-9\s]*\)", "").Trim();

            var combined = $"{artist} {songTitle}";

            var slug = string.Empty;
            
            foreach (var c in combined)
            {
                switch (c)
                {
                    case ' ':
                        slug += '-';
                        break;
                    case >= 'a' and <= 'z':
                    case >= 'A' and <= 'Z':
                    case >= '0' and <= '9':
                        slug += c;
                        break;
                    case '-':
                        slug += '-';
                        break;
                }
            }

            slug = Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');

            return slug.ToLower();
        }
    }
}
