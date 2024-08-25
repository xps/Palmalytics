using Palmalytics.Model;
using Palmalytics.Services;

namespace Palmalytics.SqlServer.Tests
{
    public static class DataSetHelpers
    {
        // 4 x Windows 10
        // 3 x iOS
        // 2 x Mac
        // 1 x Android

        // 5 x Chrome
        // 3 x Safari
        // 2 x Firefox

        private static readonly string[] userAgents = [
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36", // Chrome 121 on Windows 10
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36", // Chrome 122 on Windows 10
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:123.0) Gecko/20100101 Firefox/123.0", // Firefox 123 on Windows 10
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:124.0) Gecko/20100101 Firefox/124.0", // Firefox 124 on Windows 10
            "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1", // Safari 17.0 on iOS 17.0
            "Mozilla/5.0 (iPhone; CPU iPhone OS 17_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1 Mobile/15E148 Safari/604.1", // Safari 17.1 on iOS 17.1
            "Mozilla/5.0 (iPhone; CPU iPhone OS 17_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.2 Mobile/15E148 Safari/604.1", // Safari 17.2 on iOS 17.2
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36", // Chrome 122 on Mac 10.14
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36", // Chrome 122 on Mac 10.15
            "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Mobile Safari/537.36", // Chrome 122 on Android 10
        ];

        public static List<RequestData> GetRequests()
        {
            var requests = new List<RequestData>();

            // TODO: use DI
            var userAgentParser = new FastUserAgentParser(null);

            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 6, 30);

            // Each day, 10 users with between 1 and 3 requests each
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                for (var i = 1; i <= 10; i++)
                {
                    var ipAddress = $"{date:yyyyMMdd}.{i}";
                    var userAgent = userAgents[i - 1];
                    var device = userAgentParser.ParseUserAgent(userAgent);

                    for (var j = 1; j <= i % 3 + 1; j++)
                    {
                        var request = CreateTestRequest(
                            dateUtc: date.AddSeconds(i * 1000 + j * 10),
                            userAgent: userAgent,
                            browserName: device.BrowserName,
                            browserVersion: device.BrowserVersion,
                            osName: device.OSName,
                            osVersion: device.OSVersion,
                            ipAddress: ipAddress
                        );

                        requests.Add(request);
                    }
                }
            }

            return requests;
        }

        public static RequestData CreateTestRequest(
            DateTime? dateUtc = null,
            string? ipAddress = null,
            string? path = null,
            string? queryString = null,
            string? userAgent = null,
            string? language = null,
            string? country = null,
            string? browserName = null,
            string? browserVersion = null,
            string? osName = null,
            string? osVersion = null,
            string? referrer = null,
            string? referrerName = null,
            string? utmSource = null,
            string? utmMedium = null,
            string? utmCampaign = null,
            string? utmTerm = null,
            string? utmContent = null,
            string? userName = null,
            string? customData = null,
            int? responseCode = null,
            int? responseTime = null,
            string? contentType = null
        )
        {
            return new RequestData
            {
                //SessionId = 1,
                DateUtc = dateUtc ?? DateTime.UtcNow,
                IPAddress = ipAddress ?? "192.168.1.1",
                Path = path ?? "/products/search",
                QueryString = queryString ?? "?category=electronics&page=1",
                UserAgent = userAgent ?? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36",
                Language = language ?? "en-US",
                Country = country ?? "US",
                BrowserName = browserName ?? "Chrome",
                BrowserVersion = browserVersion ?? "90.0",
                OSName = osName ?? "Windows",
                OSVersion = osVersion ?? "10.0",
                Referrer = referrer ?? "https://www.google.com",
                ReferrerName = referrerName ?? "Google",
                UtmSource = utmSource ?? "newsletter",
                UtmMedium = utmMedium ?? "banner",
                UtmCampaign = utmCampaign ?? "summer_sale",
                UtmTerm = utmTerm ?? "electronics",
                UtmContent = utmContent ?? "image_ad",
                UserName = userName ?? "john.doe",
                CustomData = customData ?? "xyz",
                ResponseCode = responseCode ?? 200,
                ResponseTime = responseTime ?? 120,
                ContentType = contentType ?? "text/html"
            };
        }
    }
}
