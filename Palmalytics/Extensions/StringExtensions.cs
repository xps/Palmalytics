using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Palmalytics.Extensions
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Returns first characters of a string.
        /// </summary>
        public static string Left(this string str, int length)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));

            return length < str.Length ? str[..length] : str;
        }

        /// <summary>
        /// Returns the last characters of a string.
        /// </summary>
        public static string Right(this string str, int length)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));

            return length < str.Length ? str[^length..] : str;
        }

        /// <summary>
        /// Removes a substring from the start of a string, if it is present.
        /// </summary>
        public static string TrimStart(this string str, string value)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));
            if (value == null) throw new ArgumentNullException(nameof(value));

            return str.StartsWith(value) ? str[value.Length..] : str;
        }

        /// <summary>
        /// Removes a substring from the end of a string, if it is present.
        /// </summary>
        public static string TrimEnd(this string str, string value)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));
            if (value == null) throw new ArgumentNullException(nameof(value));

            return str.EndsWith(value) ? str[..^value.Length] : str;
        }

        /// <summary>
        /// Shortens a string to a maximum length, adding an ellipsis in the middle.
        /// </summary>
        public static string Shorten(this string value, int maxLength, int ellipsisPosition = 10)
        {
            if (value.Length <= maxLength)
                return value;

            return value.Left(ellipsisPosition) + "..." + value.Right(maxLength - ellipsisPosition - 3);
        }

        /// <summary>
        /// Returns null if a string or empty, or the original string if not.
        /// </summary>
        public static string NullIfEmpty(this string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
                return str;
            else
                return null;
        }

        /// <summary>
        /// Returns a string with the first character capitalized, and the rest lowercased.
        /// </summary>
        public static string Capitalize(this string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));

            if (str.Length <= 1) return str.ToUpper();

            return char.ToUpper(str[0]) + str[1..].ToLower();
        }

        /// <summary>
        /// Transforms a string in kebab-case to PascalCase.
        /// </summary>
        public static string ConvertKebabToPascalCase(this string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));

            return Regex.Replace(str, @"(^\w|-\w)", m => m.Groups[1].Value.Replace("-", "").ToUpper()).Replace("-", "");
        }

        /// <summary>
        /// Captures several values with a regex and returns them.
        /// </summary>
        public static List<string> CaptureAll(this string str, string regex, RegexOptions options = RegexOptions.None)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));

            return Regex.Matches(str, regex, options)
                    .Cast<Match>()
                    .SelectMany(m => m.Groups.Cast<Group>().Skip(1))
                    .Select(g => g.Value)
                    .ToList();
        }

        /// <summary>
        /// Captures a single (first) value with a regex and returns it.
        /// </summary>
        public static string Capture(this string str, string regex, RegexOptions options = RegexOptions.None)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));

            return str.CaptureAll(regex, options)
                .FirstOrDefault();
        }

        /// <summary>
        /// Matches a string against a 'like' pattern, where '*' is a wildcard
        /// for any number of characters, and '?' is a wildcard for a single character.
        /// </summary>
        public static bool IsLike(this string str, string pattern)
        {
            var regex = Regex.Escape(pattern)
                .Replace(@"\*", ".*")
                .Replace(@"\?", ".");

            return Regex.IsMatch(str, $"^{regex}$");
        }
    }
}
