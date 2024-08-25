using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Palmalytics.Tests.TestHelpers;

namespace Palmalytics.Tests.Integration
{
    public class DashboardTests
    {
        [Fact(Skip = "Memory data store does not implement GetBrowsers")]
        public async Task Test_Dashboard_Api_Browsers()
        {
            var webApplicationFactory = new TestWebApplicationFactory<MyStartup>();
            var client = webApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions());

            var response = await client.GetAsync("/palmalytics/api/browsers");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("Chrome");
        }

        [Fact(Skip = "Method is commented out")]
        public async Task Test_Dashboard_Api_LastRequests()
        {
            var webApplicationFactory = new TestWebApplicationFactory<MyStartup>();
            var client = webApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions());

            var response = await client.GetAsync("/palmalytics/api/last-requests");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("requests");
        }

        [Fact]
        public async Task Test_Dashboard_Index()
        {
            var webApplicationFactory = new TestWebApplicationFactory<MyStartup>();
            var client = webApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions());

            var response = await client.GetAsync("/palmalytics");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/html");
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("<!doctype html>");
        }
    }
}
