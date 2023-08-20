using System;
using System.Linq;

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

        public static string СonvertToDashedFormat(this string input, bool useExceptionSymbols = true)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;
            var result = input.ToLowerInvariant().Trim();
            result = new string(result.Select(x =>
            {
                return (char.IsLetterOrDigit(x) || (useExceptionSymbols && EXCEPTION_SYMBOLS.Contains(x))) ? x : '-';
            }).ToArray()).Replace("--", "-").Trim('-');
            return result;
        }
    }
}
