using Microsoft.Extensions.DependencyInjection;
using Palmalytics.Dashboard;
using Palmalytics.Tests.Dashboard;
using Palmalytics.Tests.TestHelpers;

namespace Palmalytics.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddPalmalytics(options =>
            {
                options.UseDataStore<MemoryDataStore>();
            });

            services.AddTransient<ApiRequestHandler>();
            services.AddTransient<DashboardRequestHandler>();
            services.AddTransient<ApiRequestHandlerTests.TestRequestHandler>();
        }
    }
}
