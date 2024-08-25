using System.Text.RegularExpressions;
using Palmalytics.Extensions;

namespace Palmalytics.Tests.Extensions
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("", 0, "")]
        [InlineData("", 5, "")]
        [InlineData("1234567890", 0, "")]
        [InlineData("1234567890", 5, "12345")]
        [InlineData("1234567890", 10, "1234567890")]
        [InlineData("1234567890", 20, "1234567890")]
        public void Test_Left(string input, int length, string output)
        {
            input.Left(length).Should().Be(output);
        }

        [Theory]
        [InlineData("", 0, "")]
        [InlineData("", 5, "")]
        [InlineData("1234567890", 0, "")]
        [InlineData("1234567890", 5, "67890")]
        [InlineData("1234567890", 10, "1234567890")]
        [InlineData("1234567890", 20, "1234567890")]
        public void Test_Right(string input, int length, string output)
        {
            input.Right(length).Should().Be(output);
        }

        [Theory]
        [InlineData("abcdef", "abc", "def")]
        [InlineData("abcdef", "def", "abcdef")]
        [InlineData("abcdef", "", "abcdef")]
        [InlineData("abcdef", "abcdef", "")]
        [InlineData("abcdef", "abcdefg", "abcdef")]
        public void Test_TrimStart(string input, string value, string output)
        {
            input.TrimStart(value).Should().Be(output);
        }

        [Theory]
        [InlineData("abcdef", "def", "abc")]
        [InlineData("abcdef", "abc", "abcdef")]
        [InlineData("abcdef", "", "abcdef")]
        [InlineData("abcdef", "abcdef", "")]
        [InlineData("abcdef", "abcdefg", "abcdef")]
        public void Test_TrimEnd(string input, string value, string output)
        {
            input.TrimEnd(value).Should().Be(output);
        }

        [Theory]
        [InlineData("Abc", "Abc")]
        [InlineData("", null)]
        [InlineData(" ", null)]
        [InlineData(null, null)]
        public void Test_NullIfEmpty(string input, string output)
        {
            input.NullIfEmpty().Should().Be(output);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("a", "A")]
        [InlineData("abc", "Abc")]
        [InlineData("AbcDEF", "Abcdef")]
        public void Test_Capitalize(string input, string output)
        {
            input.Capitalize().Should().Be(output);
        }

        [Theory]
        [InlineData("abc", "Abc")]
        [InlineData("abc-def", "AbcDef")]
        [InlineData("-abc--def-", "AbcDef")]
        [InlineData("abc-def-4-ghi", "AbcDef4Ghi")]
        public void Test_ConvertKebabToPascalCase(string input, string output)
        {
            input.ConvertKebabToPascalCase().Should().Be(output);
        }

        [Theory]
        [InlineData("en-US,en;q=0.9", @"([a-z]{2})", RegexOptions.None, new[] { "en", "en" })]
        [InlineData("en-US,en;q=0.9", @"([a-z]{2}-[A-Z]{2}),[a-z]{2};q=([0-9\.]+)", RegexOptions.None, new[] { "en-US", "0.9" })]
        [InlineData("en-US,en;q=0.9", @"Test", RegexOptions.None, new string[0])]
        public void Test_CaptureAll(string input, string regex, RegexOptions options, string[] output)
        {
            input.CaptureAll(regex, options).Should().Equal(output);
        }

        [Theory]
        [InlineData("en-US,en;q=0.9", @"^([a-z]+)", RegexOptions.None, "en")]
        [InlineData("en-US,en;q=0.9", @"^([a-z]{2}(-[a-z]{2})?)", RegexOptions.None, "en")]
        [InlineData("en-US,en;q=0.9", @"^([a-z]{2}(-[a-z]{2})?)", RegexOptions.IgnoreCase, "en-US")]
        [InlineData("en-US,en;q=0.9", @"Test", RegexOptions.None, null)]
        public void Test_Capture(string input, string regex, RegexOptions options, string output)
        {
            input.Capture(regex, options).Should().Be(output);
        }

        [Theory]
        [InlineData("abc", "?bc", true)]
        [InlineData("abc", "a?c", true)]
        [InlineData("abc", "ab?", true)]
        [InlineData("abc", "?bx", false)]
        [InlineData("abc", "x?c", false)]
        [InlineData("abc", "ax?", false)]
        [InlineData("abc/def/ghi", @"*/def/ghi", true)]
        [InlineData("abc/def/ghi", @"*/ghi", true)]
        [InlineData("abc/def/ghi", @"abc/*/ghi", true)]
        [InlineData("abc/def/ghi", @"abc/*", true)]
        [InlineData("abc/def/ghi", @"abc/def/*", true)]
        [InlineData("abc/def/ghi", @"*/dex/ghi", false)]
        [InlineData("abc/def/ghi", @"abc/*/xhi", false)]
        [InlineData("abc/def/ghi", @"abc/xef/*", false)]
        [InlineData("abcabc", @"*abc", true)]
        [InlineData("abcabc", @"abc*", true)]
        public void Test_IsLike(string input, string pattern, bool result)
        {
            input.IsLike(pattern).Should().Be(result);
        }

        [Theory]
        [InlineData("", 20, 10, "")]
        [InlineData("abc", 20, 10, "abc")]
        [InlineData("abcdefghijklmnopqrst", 20, 10, "abcdefghijklmnopqrst")]
        [InlineData("abcdefghijklmnopqrstu", 20, 10, "abcdefghij...opqrstu")]
        [InlineData("abcdefghijklmnopqrstuv", 20, 10, "abcdefghij...pqrstuv")]
        [InlineData("abcdefghijklmnopqrstuvw", 20, 10, "abcdefghij...qrstuvw")]
        [InlineData("abcdefghijklmnopqrstuvwxyz", 20, 10, "abcdefghij...tuvwxyz")]
        public void Test_Shorten(string input, int maxLength, int ellipsisPosition, string result)
        {
            input.Shorten(maxLength, ellipsisPosition).Should().Be(result);
        }
    }
}
