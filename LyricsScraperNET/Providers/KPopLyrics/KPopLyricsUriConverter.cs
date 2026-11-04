using LyricsScraperNET.Providers.Abstract;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LyricsScraperNET.Providers.KPopLyrics
{
    internal sealed class KPopLyricsUriConverter : IExternalUriConverter
    {
        internal const string Host = "www.kpoplyrics.net";

        // The WordPress search of the site: titles and links of the posts, best matches first.
        private const string SearchUriFormat = "https://" + Host + "/wp-json/wp/v2/posts?search={0}&orderby=relevance&per_page=20&_fields=title,link";

        // A part in parentheses without Latin letters and digits, for example the Hangul name in "JENNIE (제니)".
        private static readonly Regex NonLatinParenthesesRegex = new(@"\([^()a-zA-Z0-9]*\)", RegexOptions.Compiled);

        /// <summary>
        /// The site search for the Latin part of "artist song": post titles have no Hangul.
        /// Example: "JENNIE (제니)", "Mantra" searches "JENNIE Mantra".
        /// </summary>
        public Uri GetLyricUri(string artist, string song)
            => new Uri(string.Format(SearchUriFormat, Uri.EscapeDataString(GetSearchText($"{artist} {song}"))));

        public Uri GetArtistUri(string artist)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The name as post titles write it: without parts in parentheses that have no Latin letters or digits.
        /// "Back to Christmas (크리스마스처럼)" is "Back to Christmas", "XO (Only If You Say Yes)" stays as it is.
        /// </summary>
        internal static string GetTitleName(string name)
            => NonLatinParenthesesRegex.Replace(name, " ").Trim();

        /// <summary>
        /// True if the name has a Latin letter or digit: post titles have nothing else to match.
        /// </summary>
        internal static bool HasLatinLetterOrDigit(string name)
            => name.Any(IsLatinLetterOrDigit);

        // Other characters split the words: "(G)I-DLE" searches "G I DLE".
        private static string GetSearchText(string text)
        {
            var builder = new StringBuilder(text.Length);
            foreach (var c in text)
                builder.Append(IsLatinLetterOrDigit(c) ? c : ' ');

            return string.Join(" ", builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        // Basic Latin up to Latin Extended-B: "ROSÉ" and "nævis" keep their letters.
        private static bool IsLatinLetterOrDigit(char c)
            => c <= '\u024F' && char.IsLetterOrDigit(c);
    }
}
