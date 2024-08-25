using Palmalytics.Extensions;
using static Palmalytics.Tests.TestHelpers.TestRequestsHelper;

namespace Palmalytics.Tests.Extensions
{
    public class HttpRequestExtensionsTests
    {
        [Fact]
        public void Test_HttpRequestExtensions_GetDebugString()
        {
            // Arrange
            var request = CreateRequest(
                path: "/some/path?query=value",
                headers: new() {
                    { "Header1", "Value1" },
                    { "Header2", "Value2" },
                    { "User-Agent", "Google Chrome" },
                    { "Accept-Language", "en-US" }
                }
            );

            // Act
            var lines = request.GetDebugString();

            // Assert
            lines.Split(Environment.NewLine).Should().Equal(
                "GET /some/path?query=value",
                "Header1: Value1",
                "Header2: Value2",
                "User-Agent: Google Chrome",
                "Accept-Language: en-US"
            );
        }
    }
}
