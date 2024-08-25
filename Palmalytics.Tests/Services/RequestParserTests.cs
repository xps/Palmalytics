using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Palmalytics.Services;
using static Palmalytics.Tests.TestHelpers.TestRequestsHelper;

namespace Palmalytics.Tests.Services
{
    public class RequestParserTests
    {
        private readonly IServiceProvider serviceProvider;

        public RequestParserTests(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [Fact]
        public void Test_RequestParser_Parse_Basics()
        {
            // Arrange
            var options = new PalmalyticsParserOptions();
            var parser = CreateRequestParser(options);

            var request = CreateRequest(
                path: "/some/path?query=value",
                ipAddress: "210.175.122.187",
                headers: new() { { "Referer", "https://www.google.com" } }
            );

            // Act
            var result = parser.Parse(request);

            // Assert
            result.DateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.IPAddress.Should().Be("210.175.122.187");
            result.Path.Should().Be("/some/path");
            result.QueryString.Should().Be("query=value");
            result.Referrer.Should().Be("https://www.google.com");
        }

        [Fact]
        public void Test_RequestParser_Parse_With_Missing_Data()
        {
            // Arrange
            var options = new PalmalyticsParserOptions
            {
                CollectLanguage = true,
                CollectUserAgent = true
            };

            var parser = CreateRequestParser(options);

            var request = CreateRequest(
                path: "/some/path",
                ipAddress: "210.175.122.187",
                userAgent: null,
                acceptLanguage: null
            );

            // Act
            var result = parser.Parse(request);

            // Assert
            result.DateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.IPAddress.Should().Be("210.175.122.187");
            result.Path.Should().Be("/some/path");
            result.QueryString.Should().BeNull();
            result.UserAgent.Should().BeNull();
            result.Language.Should().BeNull();
            result.Referrer.Should().BeNull();
        }

        [Fact]
        public void Test_RequestParser_CollectUtmParameters_True()
        {
            // Arrange
            var options = new PalmalyticsParserOptions { CollectUtmParameters = true };
            var parser = CreateRequestParser(options);

            var parameters = new Dictionary<string, string>
            {
                { "utm_medium", "some medium" },
                { "utm_source", "some source" },
                { "utm_campaign", "some campaign" },
                { "utm_term", "some term" },
                { "utm_content", "some content" }
            };

            var queryString = string.Join("&", parameters.Select(x => $"{x.Key}={WebUtility.UrlEncode(x.Value)}"));

            var request = CreateRequest(
                path: $"/some/path?{queryString}"
            );

            // Act
            var result = parser.Parse(request);

            // Assert
            result.UtmSource.Should().Be("some source");
            result.UtmMedium.Should().Be("some medium");
            result.UtmCampaign.Should().Be("some campaign");
            result.UtmTerm.Should().Be("some term");
            result.UtmContent.Should().Be("some content");
        }

        [Fact]
        public void Test_RequestParser_CollectUtmParameters_False()
        {
            // Arrange
            var options = new PalmalyticsParserOptions { CollectUtmParameters = false };
            var parser = CreateRequestParser(options);

            var parameters = new Dictionary<string, string>
            {
                { "utm_medium", "some medium" },
                { "utm_source", "some source" },
                { "utm_campaign", "some campaign" },
                { "utm_term", "some term" },
                { "utm_content", "some content" }
            };

            var queryString = string.Join("&", parameters.Select(x => $"{x.Key}={WebUtility.UrlEncode(x.Value)}"));

            var request = CreateRequest(
                path: $"/some/path?{queryString}"
            );

            // Act
            var result = parser.Parse(request);

            // Assert
            result.UtmSource.Should().BeNull();
            result.UtmMedium.Should().BeNull();
            result.UtmCampaign.Should().BeNull();
            result.UtmTerm.Should().BeNull();
            result.UtmContent.Should().BeNull();
        }

        [Fact]
        public void Test_RequestParser_Parse_Language()
        {
            // Arrange
            var options = new PalmalyticsParserOptions { CollectLanguage = true };
            var parser = CreateRequestParser(options);

            var request = CreateRequest(acceptLanguage: "en-US;q=0.9");

            // Act
            var result = parser.Parse(request);

            // Assert
            result.Language.Should().Be("en-US");
        }

        [Fact]
        public void Test_RequestParser_Parse_Browser_And_OS()
        {
            // Arrange
            var options = new PalmalyticsParserOptions();
            var parser = CreateRequestParser(options);

            var request = CreateRequest(
                userAgent: "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
                headers: new()
                {
                    { "Sec-CH-UA", """"Not/A)Brand";v="99", "Google Chrome";v="116", "Chromium";v="116"""" },
                    { "Sec-CH-UA-Platform", "Windows" },
                    //{ "Sec-CH-UA-Platform-Version", "15.0.0" }, // TODO
                    { "Sec-CH-UA-Mobile", "?0" },
                }
            );

            // Act
            var result = parser.Parse(request);

            // Assert
            result.BrowserName.Should().Be("Chrome");
            result.BrowserVersion.Should().Be("116");
            result.OSName.Should().Be("Windows");
            //result.OSVersion.Should().Be("11");
        }

        [Fact]
        public void Test_RequestParser_Parse_CF_Connecting_IP()
        {
            // Arrange
            var options = new PalmalyticsParserOptions { CollectIPAddress = true };
            var parser = CreateRequestParser(options);

            var request = CreateRequest(
                headers: new()
                {
                    { "CF-Connecting-IP", "152.12.65.25" },
                    { "X-Forwarded-For", "152.12.65.25, 158.64.59.175" }
                }
            );

            // Act
            var result = parser.Parse(request);

            // Assert
            result.IPAddress.Should().Be("152.12.65.25");
        }

        [Fact]
        public void Test_RequestParser_Parse_X_Forwarded_For()
        {
            // Arrange
            var options = new PalmalyticsParserOptions { CollectIPAddress = true };
            var parser = CreateRequestParser(options);

            var request = CreateRequest(
                headers: new()
                {
                    { "X-Forwarded-For", "152.12.65.25" }
                }
            );

            // Act
            var result = parser.Parse(request);

            // Assert
            result.IPAddress.Should().Be("152.12.65.25");
        }

        #region Options tests

        [Fact]
        public void Test_RequestParser_CollectQueryString_True()
        {
            // Arrange
            var options = new PalmalyticsParserOptions { CollectQueryString = true };
            var parser = CreateRequestParser(options);
            var request = CreateRequest(path: "/search?q=test&page=4");

            // Act
            var result = parser.Parse(request);

            // Assert
            result.QueryString.Should().Be("q=test&page=4");
        }

        [Fact]
        public void Test_RequestParser_CollectQueryString_False()
        {
            // Arrange
            var options = new PalmalyticsParserOptions { CollectQueryString = false };
            var parser = CreateRequestParser(options);
            var request = CreateRequest(path: "/search?q=test&page=4");

            // Act
            var result = parser.Parse(request);

            // Assert
            result.QueryString.Should().BeNull();
        }

        [Fact]
        public void Test_RequestParser_CollectIPAddress_True()
        {
            // Arrange
            var options = new PalmalyticsParserOptions { CollectIPAddress = true };
            var parser = CreateRequestParser(options);
            var request = CreateRequest();

            // Act
            var result = parser.Parse(request);

            // Assert
            result.IPAddress.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Test_RequestParser_CollectIPAddress_False()
        {
            // Arrange
            var options = new PalmalyticsParserOptions { CollectIPAddress = false };
            var parser = CreateRequestParser(options);
            var request = CreateRequest();

            // Act
            var result = parser.Parse(request);

            // Assert
            result.IPAddress.Should().BeNull();
        }

        [Fact]
        public void Test_RequestParser_CollectUserAgent_True()
        {
            // Arrange
            var options = new PalmalyticsParserOptions { CollectUserAgent = true };
            var parser = CreateRequestParser(options);
            var request = CreateRequest();

            // Act
            var result = parser.Parse(request);

            // Assert
            result.UserAgent.Should().NotBeNullOrWhiteSpace();
            result.BrowserName.Should().NotBeNullOrWhiteSpace();
            result.BrowserVersion.Should().NotBeNullOrWhiteSpace();
            result.OSName.Should().NotBeNullOrWhiteSpace();
            result.OSVersion.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Test_RequestParser_CollectUserAgent_False()
        {
            // Arrange
            var options = new PalmalyticsParserOptions { CollectUserAgent = false };
            var parser = CreateRequestParser(options);
            var request = CreateRequest();

            // Act
            var result = parser.Parse(request);

            // Assert
            result.UserAgent.Should().BeNull();
            result.BrowserName.Should().BeNull();
            result.BrowserVersion.Should().BeNull();
            result.OSName.Should().BeNull();
            result.OSVersion.Should().BeNull();
        }

        [Fact]
        public void Test_RequestParser_CollectLanguage_True()
        {
            // Arrange
            var options = new PalmalyticsParserOptions { CollectLanguage = true };
            var parser = CreateRequestParser(options);
            var request = CreateRequest();

            // Act
            var result = parser.Parse(request);

            // Assert
            result.Language.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Test_RequestParser_CollectLanguage_False()
        {
            // Arrange
            var options = new PalmalyticsParserOptions { CollectLanguage = false };
            var parser = CreateRequestParser(options);
            var request = CreateRequest();

            // Act
            var result = parser.Parse(request);

            // Assert
            result.Language.Should().BeNull();
        }

        [Fact(Skip = "This test needs geolocalization to be set up")]
        public void Test_RequestParser_CollectCountry_True()
        {
            // Arrange
            var options = new PalmalyticsParserOptions { CollectCountry = true };
            var parser = CreateRequestParser(options);
            var request = CreateRequest();

            // Act
            var result = parser.Parse(request);

            // Assert
            result.Country.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Test_RequestParser_CollectCountry_False()
        {
            // Arrange
            var options = new PalmalyticsParserOptions { CollectCountry = false };
            var parser = CreateRequestParser(options);
            var request = CreateRequest();

            // Act
            var result = parser.Parse(request);

            // Assert
            result.Country.Should().BeNull();
        }

        [Fact]
        public void Test_RequestParser_CollectUserName_True()
        {
            // Arrange
            var options = new PalmalyticsParserOptions { CollectUserName = true };
            var parser = CreateRequestParser(options);
            var request = CreateRequest();

            // Act
            var result = parser.Parse(request);

            // Assert
            result.UserName.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Test_RequestParser_CollectUserName_False()
        {
            // Arrange
            var options = new PalmalyticsParserOptions { CollectUserName = false };
            var parser = CreateRequestParser(options);
            var request = CreateRequest();

            // Act
            var result = parser.Parse(request);

            // Assert
            result.UserName.Should().BeNull();
        }

        #endregion

        #region Language tests

        [Theory]
        [InlineData("*", null)]
        [InlineData("en-US,en;q=0.9", "en-US")]
        [InlineData("en-us,en;q=0.9", "en-US")]
        [InlineData("en", "en")]
        [InlineData("en-GB", "en-GB")]
        [InlineData("en-GB,en-US;q=0.9,en;q=0.8", "en-GB")]
        [InlineData("zh-cn", "zh-CN")]
        [InlineData("en; q=1.0", "en")]
        [InlineData("en,nl;q=0.9,en-US;q=0.8,de;q=0.7", "en")]
        [InlineData("fr,fr-FR;q=0.8,en-US;q=0.5,en;q=0.3", "fr")]
        [InlineData("de,*", "de")]
        [InlineData("de,en,*", "de")]
        [InlineData("es-419,es;q=0.9", "es")]
        public void Test_RequestParser_ParseLanguage(string input, string output)
        {
            // Arrange
            var options = new PalmalyticsParserOptions { CollectLanguage = true };
            var parser = CreateRequestParser(options);

            // Act
            var language = parser.ParseLanguage(input);

            // Assert
            language.Should().Be(output);
        }

        #endregion

        private RequestParser CreateRequestParser(PalmalyticsParserOptions options)
        {
            return ActivatorUtilities.CreateInstance<RequestParser>(serviceProvider, Options.Create(options));
        }
    }
}
