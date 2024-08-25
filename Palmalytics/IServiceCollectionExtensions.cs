using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Palmalytics.Dashboard;
using Palmalytics.Exceptions;
using Palmalytics.Middleware;
using Palmalytics.Services;

namespace Palmalytics
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddPalmalytics(this IServiceCollection services, Action<PalmalyticsOptions> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddSingleton<IRequestParser, RequestParser>();
            services.AddSingleton<IRequestFilter, RequestFilter>();
            services.AddSingleton<IResponseFilter, ResponseFilter>();
            services.AddSingleton<IReferrerParser, ReferrerParser>();
            services.AddSingleton<IUserAgentParser, FastUserAgentParser>();
            services.AddSingleton<IApiRequestHandler, ApiRequestHandler>();
            services.AddSingleton<IDashboardRequestHandler, DashboardRequestHandler>();
            services.AddSingleton<TrackingMiddleware>();

            services.AddTransient<IGeocodingDataProvider, GeocodingDataProvider>();
            services.AddTransient(x => Options.Create(x.GetRequiredService<IOptions<PalmalyticsOptions>>().Value.ParserOptions));
            services.AddTransient(x => Options.Create(x.GetRequiredService<IOptions<PalmalyticsOptions>>().Value.FilterOptions));

            services.AddSingleton(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<PalmalyticsOptions>>().Value;

                if (options.DataStoreType == null)
                    throw new PalmalyticsOptionsException("No data store configured");

                var dataStore = ActivatorUtilities.CreateInstance(serviceProvider, options.DataStoreType, Options.Create(options.DataStoreOptions)) as IDataStore ??
                    throw new InvalidOperationException($"Could not create instance of {options.DataStoreType}");

                return dataStore;
            });

            if (configure != null)
                services.Configure(configure);

            return services;
        }
    }
}
