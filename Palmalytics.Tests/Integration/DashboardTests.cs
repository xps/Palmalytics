using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Palmalytics.Tests.TestHelpers;

namespace Palmalytics.Tests.Integration
{
    public class DashboardTests
    {
        [Fact]
        public async Task Test_Dashboard_Api_Version()
        {
            var webApplicationFactory = new TestWebApplicationFactory<MyStartup>();
            var client = webApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions());

            var response = await client.GetAsync("/palmalytics/api/version");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("version");
        }

        [Theory]
        [InlineData("/palmalytics/api/type")]
        [InlineData("/palmalytics/api/hash-code")]
        [InlineData("/palmalytics/api/does-not-exist")]
        public async Task Test_Dashboard_Api_404s(string path)
        {
            var webApplicationFactory = new TestWebApplicationFactory<MyStartup>();
            var client = webApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions());

            var response = await client.GetAsync(path);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
