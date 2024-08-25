using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Palmalytics.Tests.TestHelpers
{
    public class MyStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddPalmalytics(options =>
            {
                options.AutomaticallyDownloadGeocodingData = false;
                options.FilterOptions.IgnoreSegments.Add("/ignore");
                options.ParserOptions.CollectIPAddress = false;
                options.DashboardOptions.Authorize = null;
                options.UseDataStore<MemoryDataStore>();
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UsePalmalytics();

            app.Use(app =>
            {
                return async (HttpContext context) =>
                {
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync(@"
                        <DOCTYPE html>
                        <html>
                            <head>
                                <title>Test</title>
                            </head>
                            <body>
                                <h1>Test</h1>
                            </body>
                        </html>
                    ");
                };
            });
        }
    }
}
