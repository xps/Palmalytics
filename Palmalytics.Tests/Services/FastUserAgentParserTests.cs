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
                    { "Sec-CH-UA", "\"Not/A)Brand\";v=\"99\", \"Google Chrome\";v=\"116\", \"Chromium\";v=\"116\"" },
                    { "Sec-CH-UA-Platform", "\"Windows\"" },
                    // { "Sec-CH-UA-Platform-Version", "\"15.0.0\"" }, // TODO
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

        [Fact]
        public void Test_FastUserAgentParser_Parses_ClientHints_In_LowerCase()
        {
            var request = CreateRequest(headers: new()
            {
                { "sec-ch-ua", "\"Not/A)Brand\";v=\"99\", \"Google Chrome\";v=\"115\", \"Chromium\";v=\"115\"" },
                { "sec-ch-ua-platform", "\"Windows\"" },
                { "sec-ch-ua-platform-version", "\"11\"" },
                { "sec-ch-ua-mobile", "?0" },
            });

            var device = parser.GetDevice(request);

            device.BrowserName.Should().Be("Chrome");
            device.BrowserVersion.Should().Be("115");
            device.OSName.Should().Be("Windows");
            device.OSVersion.Should().Be("11");
        }

        [Theory]
        [InlineData("\"Samsung Internet\";v=\"30.0\", \"Chromium\";v=\"143\", \"Not A(Brand\";v=\"24\"", "Samsung Internet", "30.0")]
        [InlineData("\"Not=A?Brand\";v=\"99\", \"Opera GX\";v=\"135\", \"Chromium\";v=\"151\"", "Opera", "135")]
        [InlineData("\"OperaMobile\";v=\"101\", \" Not;A Brand\";v=\"99\", \"Chromium\";v=\"151\", \"Opera\";v=\"136\"", "Opera", "136")]
        [InlineData("\"Not=A?Brand\";v=\"99\", \"Opera\";v=\"135\", \"Chromium\";v=\"151\"", "Opera", "135")]
        [InlineData("\"Not=A?Brand\";v=\"99\", \"DuckDuckGo\";v=\"151\", \"Chromium\";v=\"151\"", "DuckDuckGo", "151")]
        [InlineData("\"Chromium\";v=\"152\", \"Not?A_Brand\";v=\"24\", \"Vivaldi\";v=\"8.2\"", "Vivaldi", "8.2")]
        [InlineData("\"Not;A=Brand\";v=\"8\", \"Chromium\";v=\"150\", \"Yandex\";v=\"26\"", "Yandex", "26")]
        [InlineData("\"XiaoMiBrowser\";v=\"135\", \"Not-A.Brand\";v=\"8\", \"Chromium\";v=\"135\"", "Xiaomi Browser", "135")]
        [InlineData("\"Chromium\";v=\"119\", \"Not?A_Brand\";v=\"24\"", "Chrome", "119")]
        [InlineData("\"Not=A?Brand\";v=\"99\", \"Android WebView\";v=\"151\", \"Chromium\";v=\"151\"", "Chrome", "151")]
        [InlineData("\"not/a)brand\";v=\"99\", \"google chrome\";v=\"116\", \"chromium\";v=\"116\"", "Chrome", "116")]
        public void Test_FastUserAgentParser_Parses_ClientHints_Browsers(string secChUa, string browserName, string browserVersion)
        {
            var device = parser.ParseClientHints(CreateRequest(headers: new() { { "Sec-CH-UA", secChUa } }).Headers);

            device.IsBot.Should().Be(false);
            device.BrowserName.Should().Be(browserName);
            device.BrowserVersion.Should().Be(browserVersion);
        }

        [Fact]
        public void Test_FastUserAgentParser_Generic_Chromium_ClientHints_Does_Not_Override_UserAgent_Browser()
        {
            var request = CreateRequest(
                userAgent: "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Whale/4.33.325.17 Safari/537.36",
                headers: new() { { "Sec-CH-UA", "\"Chromium\";v=\"140\", \"Not=A?Brand\";v=\"24\"" } });

            var device = parser.GetDevice(request);

            device.BrowserName.Should().Be("Whale");
            device.BrowserVersion.Should().Be("4");
        }

        [Fact]
        public void Test_FastUserAgentParser_Detects_HeadlessChrome_ClientHints_As_Bot()
        {
            var request = CreateRequest(
                userAgent: "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36",
                headers: new() { { "Sec-CH-UA", "\"HeadlessChrome\";v=\"141\", \"Not?A_Brand\";v=\"8\", \"Chromium\";v=\"141\"" } });

            var device = parser.GetDevice(request);

            device.IsBot.Should().Be(true);
        }
    }
}
