using HttpContextMoq;
using HttpContextMoq.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Palmalytics.Tests.TestHelpers
{
    public static class TestResponsesHelper
    {
        /// <summary>
        /// Create a response that by default passes the default filter
        /// </summary>
        public static HttpResponse CreateResponse(
            int statusCode = 200,
            string contentType = "text/html",
            int contentLength = 100_000,
            Dictionary<string, StringValues> headers = null)
        {
            headers ??= [];

            var context = new HttpContextMock()
                .SetupResponseStatusCode(statusCode)
                .SetupResponseContentType(contentType)
                .SetupResponseContentLength(contentLength)
                .SetupResponseHeaders(headers);

            return context.Response;
        }
    }
}
