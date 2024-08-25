using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Palmalytics.Tests.TestHelpers;

namespace Palmalytics.Tests.Integration
{
    // See: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0
    public class ApplicationBuilderTests
    {
        [Fact(Skip = "Doesn't work when other tests run in parallel")]
        public async Task Test_TrackingMiddleware_Is_Called()
        {
            var webApplicationFactory = new TestWebApplicationFactory<MyStartup>();

            var client = webApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions());

            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");

            await client.GetAsync("/test-abc");

            var dataStore = webApplicationFactory.Services.GetRequiredService<IDataStore>();
            var requests = dataStore.GetLastRequests();

            requests.Should().HaveCount(1);
            requests[0].Path.Should().Be("/test-abc");
        }

        [Fact]
        public async Task Test_TrackingMiddleware_Uses_Configuration()
        {
            var webApplicationFactory = new TestWebApplicationFactory<MyStartup>();
            var client = webApplicationFactory.CreateClient();

            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");

            await client.GetAsync("/ignore");

            var dataStore = webApplicationFactory.Services.GetRequiredService<IDataStore>();
            var requests = dataStore.GetLastRequests();

            requests.Should().BeEmpty();
        }
    }
}
