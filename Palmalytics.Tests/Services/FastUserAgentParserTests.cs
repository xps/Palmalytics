using Palmalytics.Extensions;
using Palmalytics.Services;
using Palmalytics.Tests.TestHelpers;
using static Palmalytics.Tests.TestHelpers.TestRequestsHelper;

namespace Palmalytics.Tests.Services
{
    public class FastUserAgentParserTests
    {
        private readonly FastUserAgentParser parser;

        public FastUserAgentParserTests(IUserAgentParser parser)
        {
            this.parser = (FastUserAgentParser)parser;
        }

        [Fact]
        public void Test_FastUserAgentParser_Parses_ClientHints()
        {
            var request = CreateRequest(
                userAgent: "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
                headers: new()
                {
                    { "Sec-CH-UA", """"Not/A)Brand";v="99", "Google Chrome";v="116", "Chromium";v="116"""" },
                    { "Sec-CH-UA-Platform", "Windows" },
                    // { "Sec-CH-UA-Platform-Version", "15.0.0" }, // TODO
                    { "Sec-CH-UA-Mobile", "?0" },
                }
            );

            var device = parser.GetDevice(request);

            device.IsBot.Should().Be(false);
            device.BrowserName.Should().Be("Chrome");
            device.BrowserVersion.Should().Be("116");
            device.OSName.Should().Be("Windows");
            //device.OSVersion.Should().Be("11");
        }

        [Theory]
        [CsvData("UserAgents-Bots.csv")]
        public void Test_FastUserAgentParser_Parses_Bots(string userAgent, string botName)
        {
            var request = CreateRequest(userAgent: userAgent);
            var device = parser.GetDevice(request);

            device.IsBot.Should().Be(true, $"'{botName}' is a bot");
        }

        [Theory]
        [CsvData("UserAgents-TopBrowsers.csv")]
        public void Test_FastUserAgentParser_Parses_All_Top_Browsers(string description, string userAgent, string browserName, string browserVersion, string osName, string osVersion, bool isMobile)
        {
            // We could also test with this data: https://www.useragents.me/

            var request = CreateRequest(userAgent: userAgent);
            var device = parser.GetDevice(request);

            description.Should().NotBeNullOrWhiteSpace();

            device.IsBot.Should().Be(false);
            device.BrowserName.Should().Be(browserName);
            device.BrowserVersion.Should().Be(browserVersion);
            device.OSName.Should().Be(osName);
            device.OSVersion.Should().Be(osVersion?.NullIfEmpty());
            device.IsMobile.Should().Be(isMobile);
        }

        [Fact(Skip = "HttpRequestMoq needs to be fixed as it reads headers case-sensitively")]
        public void Test_FastUserAgentParser_Parses_ClientHints_In_LowerCase()
        {
            var request = CreateRequest(headers: new()
            {
                { "sec-ch-ua", """"Not/A)Brand";v="99", "Google Chrome";v="115", "Chromium";v="115"""" },
                { "sec-ch-ua-platform", "Windows" },
                { "sec-ch-ua-platform-version", "11" },
                { "sec-ch-ua-mobile", "?0" },
            });

            var device = parser.GetDevice(request);

            device.BrowserName.Should().Be("Chrome");
            device.BrowserVersion.Should().Be("115");
            device.OSName.Should().Be("Windows");
            device.OSVersion.Should().Be("11");
        }
    }
}
