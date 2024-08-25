using System.Net;

namespace Palmalytics.Tests.TestHelpers
{
    public class HelpersTests
    {
        [Fact]
        public void Test_CreateRequest_Basics()
        {
            var request = TestRequestsHelper.CreateRequest(
                path: "/path",
                method: "GET",
                ipAddress: "125.0.95.61",
                userAgent: "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
                acceptLanguage: "en-US,en;q=0.9",
                userName: "john.doe",
                headers: new()
                {
                    {  "Header", "Value" }
                }
            );

            request.Path.Value.Should().Be("/path");
            request.Method.Should().Be("GET");
            request.HttpContext.Connection.RemoteIpAddress.Should().Be(IPAddress.Parse("125.0.95.61"));
            request.Headers["User-Agent"].Should().BeEquivalentTo("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
            request.Headers["Accept-Language"].Should().BeEquivalentTo("en-US,en;q=0.9");
            request.Headers["Header"].Should().BeEquivalentTo("Value");
            request.HttpContext.User.Identity.Should().NotBeNull();
            request.HttpContext.User.Identity!.Name.Should().Be("john.doe");
        }

        [Fact]
        public void Test_CreateRequest_Headers_Should_Be_Case_Insensitive()
        {
            var request = TestRequestsHelper.CreateRequest(
                headers: new()
                {
                    {  "Header", "Value" }
                }
            );

            request.Headers["Header"].Should().BeEquivalentTo("Value");
            request.Headers["header"].Should().BeEquivalentTo("Value");
        }

        [Fact(Skip = "Not supported by HttpRequestMoq")]
        public void Test_CreateRequest_Headers_Should_Support_Multiple_Values()
        {
            var request = TestRequestsHelper.CreateRequest(
                headers: new()
                {
                    {  "Header", "Value1" },
                    {  "Header", "Value2" }
                }
            );

            request.Headers["Header"].Should().BeEquivalentTo("Value1", "Value2");
            request.Headers["Header"].ToString().Should().Be("Value1,Value2");
        }

        [Fact]
        public void Test_CreateRequest_Headers_Should_Support_Items()
        {
            var request = TestRequestsHelper.CreateRequest();

            request.HttpContext.Items.Add("Key", "Value");

            request.HttpContext.Items["Key"].Should().Be("Value");
        }
    }
}
