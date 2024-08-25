using Palmalytics.Services;

namespace Palmalytics.Tests.Services
{
    public class ReferrerParserTests
    {
        private readonly IReferrerParser referrerParser;

        public ReferrerParserTests(IReferrerParser referrerParser)
        {
            this.referrerParser = referrerParser;

            referrerParser.Initialize([
                "au",
                "com",
                "com.au",
                "blogger.com",
                "blogger.com.au"
            ]);
        }

        [Theory]
        [InlineData("https://google.com", "Google")]
        [InlineData("https://www.google.com", "Google")]
        [InlineData("https://blog.google.com", "Google")]
        [InlineData("https://google.com.au", "Google")]
        [InlineData("https://www.google.com.au", "Google")]
        [InlineData("https://blog.google.com.au", "Google")]
        [InlineData("https://blogger.com", "blogger.com")]
        [InlineData("https://blogger.com.au", "blogger.com.au")]
        [InlineData("https://www.blogger.com", "blogger.com")]
        [InlineData("https://www.blogger.com.au", "blogger.com.au")]
        [InlineData("https://bob.blogger.com", "bob.blogger.com")]
        [InlineData("https://bob.blogger.com.au", "bob.blogger.com.au")]
        [InlineData("https://somesite.com/some-url", "somesite.com")]
        [InlineData("https://somesite.com?utm_source=someothersite.com", "somesite.com")]
        [InlineData("https://72.72.72.72", "72.72.72.72")]
        public void Test_ReferrerParser_Returns_Correct_Registrable_Domain(string domain, string expected)
        {
            // Act
            var result = referrerParser.GetReferrerName(domain);

            // Assert
            result.Should().Be(expected);
        }
    }
}
