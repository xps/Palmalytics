using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Palmalytics.Dashboard;
using Palmalytics.Middleware;

namespace Palmalytics
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UsePalmalytics(this IApplicationBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.UseMiddleware<TrackingMiddleware>();

            builder.Map("/palmalytics/api", appBuilder =>
            {
                var handler = appBuilder.ApplicationServices.GetRequiredService<IApiRequestHandler>();
                appBuilder.Run(handler.HandleRequestAsync);
            });

            builder.Map("/palmalytics", appBuilder =>
            {
                var handler = appBuilder.ApplicationServices.GetRequiredService<IDashboardRequestHandler>();
                appBuilder.Run(handler.HandleRequestAsync);
            });

            return builder;
        }
    }
}
