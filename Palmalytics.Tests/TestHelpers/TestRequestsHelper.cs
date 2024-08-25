using System.Net;
using HttpContextMoq;
using HttpContextMoq.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Palmalytics.Tests.TestHelpers
{
    public static class TestRequestsHelper
    {
        /// <summary>
        /// Create a requests that by default passes the default filter
        /// </summary>
        public static HttpRequest CreateRequest(
            string path = "/path",
            string method = "GET",
            string ipAddress = "125.0.95.61",
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
            string acceptLanguage = "en-US,en;q=0.9",
            string userName = "john.doe",
            Dictionary<string, StringValues> headers = null)
        {
            headers ??= [];

            if (!headers.ContainsKey("User-Agent") && userAgent != null)
                headers.Add("User-Agent", userAgent);

            if (!headers.ContainsKey("Accept-Language") && acceptLanguage != null)
                headers.Add("Accept-Language", acceptLanguage);

            var context = new HttpContextMock()
                .SetupUrl("https://www.testsite.com" + path)
                .SetupRequestMethod(method)
                .SetupRequestHeaders(headers);

            if (ipAddress != null)
                context.ConnectionMock.Mock.SetupGet(x => x.RemoteIpAddress).Returns(IPAddress.Parse(ipAddress));

            if (!string.IsNullOrWhiteSpace(userName))
                context.UserMock.IdentityMock.Mock.SetupGet(x => x.Name).Returns(userName);

            context.Items = new ItemsDictionaryFake();

            return context.Request;
        }
    }
}
