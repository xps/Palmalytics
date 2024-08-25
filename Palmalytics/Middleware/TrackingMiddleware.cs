using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Palmalytics.Exceptions;
using Palmalytics.Model;
using Palmalytics.Services;
using Palmalytics.Utilities.Collections;

namespace Palmalytics.Middleware
{
    public class TrackingMiddleware : IMiddleware
    {
        private readonly IRequestParser requestParser;
        private readonly IRequestFilter requestFilter;
        private readonly IResponseFilter responseFilter;
        private readonly PalmalyticsOptions options;
        private readonly IDataStore dataStore;
        private readonly ILogger logger;

        private static readonly RingBuffer<int> performanceStats = new(100);
        private static int successiveFailCount;

        public IDataStore DataStore => dataStore;
        public PalmalyticsOptions Options => options;

        public static int SuccessiveFailCount => successiveFailCount;
        public static int[] PerformanceStats => performanceStats.ToArray();

        public TrackingMiddleware(
            IRequestParser requestParser,
            IRequestFilter requestFilter,
            IResponseFilter responseFilter,
            IReferrerParser referrerParser,
            IServiceScopeFactory serviceScopeFactory,
            IOptions<PalmalyticsOptions> options,
            IDataStore dataStore,
            ILogger<TrackingMiddleware> logger)
        {
            this.requestParser = requestParser;
            this.requestFilter = requestFilter;
            this.responseFilter = responseFilter;
            this.dataStore = dataStore;
            this.options = options.Value;
            this.logger = logger;

            logger.LogDebug("Initializing referrer parser");
            referrerParser.Initialize();

            logger.LogDebug("Initializing data store");
            dataStore.Initialize();

            // Check if the geoloc database needs to be updated
            if (this.options.AutomaticallyDownloadGeocodingData)
            {
                var settings = dataStore.GetSettings();
                var version = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM");
                if (string.IsNullOrWhiteSpace(settings.GeocodingDataVersion) || StringComparer.Ordinal.Compare(settings.GeocodingDataVersion, version) < 0)
                {
                    logger.LogInformation("Updating geocoding database (current version: {currentVersion}, new version: {newVersion})", settings.GeocodingDataVersion, version);
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            logger.LogDebug("Downloading geocoding data from source");
                            using var scope = serviceScopeFactory.CreateScope();
                            var geocodingDataProvider = scope.ServiceProvider.GetRequiredService<IGeocodingDataProvider>();
                            var geocodingData = await geocodingDataProvider.DownloadGeocodingDataAsync(version);
                            dataStore.ImportGeocodingData(geocodingData);
                            dataStore.SaveSetting("GeocodingDataVersion", version);
                            logger.LogDebug("Geocoding data updated to version {version}", version);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error while updating geocoding database");
                        }
                    });
                }
                else
                {
                    logger.LogDebug("Geocoding database is up to date ({version})", settings.GeocodingDataVersion);
                }
            }
        }

        public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
        {
            // Filter request
            var palmalyticsStopwatch = Stopwatch.StartNew();
            var shouldTrackRequest = false;
            try
            {
                shouldTrackRequest = requestFilter.ShouldTrackRequest(httpContext.Request);
            }
            catch (Exception ex)
            {
                HandleException(ex, "Error in request filter");
            }

            // Parse request
            var data = null as RequestData;
            if (shouldTrackRequest && options.EnableTracking)
            {
                try
                {
                    data = requestParser.Parse(httpContext.Request);
                }
                catch (Exception ex)
                {
                    HandleException(ex, "Error in request parser");
                    shouldTrackRequest = false;
                }
            }

            // Exclude bots
            if (data?.IsBot == true && options.FilterOptions.IgnoreBots)
                shouldTrackRequest = false;

            // Geocoding
            if (shouldTrackRequest && options.EnableTracking && options.ParserOptions.CollectCountry)
            {
                try
                {
                    var address = options.GetClientIPAddress(httpContext.Request);
                    data.Country = dataStore.GetCountryCodeForIPAddress(address);
                }
                catch (Exception ex)
                {
                    HandleException(ex, "Error in geocoding");
                }
            }

            // Call parsing event handler
            try
            {
                options.OnParseRequest?.Invoke(this, new OnParseRequestEventArgs(httpContext, data));
            }
            catch (Exception ex)
            {
                HandleException(ex, "Error in OnParseRequest event handler");
                shouldTrackRequest = false;
            }

            // Run the rest of the pipeline
            palmalyticsStopwatch.Stop();
            var responseStopwatch = Stopwatch.StartNew();
            await next(httpContext);
            responseStopwatch.Stop();
            palmalyticsStopwatch.Start();

            // Continue only if data was successfully parsed
            var saved = false;
            if (data != null && shouldTrackRequest && options.EnableTracking)
            {
                // Filter response
                var shouldTrackResponse = false;
                try
                {
                    shouldTrackResponse = responseFilter.ShouldTrackResponse(httpContext.Response);
                }
                catch (Exception ex)
                {
                    HandleException(ex, "Error in response filter");
                }

                if (shouldTrackResponse)
                {
                    // Read response data
                    data.ResponseTime = (int)responseStopwatch.ElapsedMilliseconds;
                    data.ResponseCode = httpContext.Response.StatusCode;
                    data.ContentType = httpContext.Response.ContentType;

                    try
                    {
                        // Call event handler
                        options.OnBeforeSavingRequest?.Invoke(this, new OnBeforeSavingRequestEventArgs(httpContext, data));
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex, "Error in OnBeforeSavingRequest event handler");
                    }

                    // Save request
                    if (options.EnableAsyncWrites)
                    {
                        _ = Task.Run(async () =>
                        {
                            // NOTE: we're capturing the middleware and the datastore in the closure,
                            // but it should be ok since they're singletons
                            try
                            {
                                await dataStore.AddRequestAsync(data);
                                successiveFailCount = 0;
                            }
                            catch (Exception ex)
                            {
                                HandleException(ex, "Error while saving request");
                            }
                        });
                        saved = true;
                    }
                    else
                    {
                        try
                        {
                            await dataStore.AddRequestAsync(data);
                            successiveFailCount = 0;
                            saved = true;
                        }
                        catch (Exception ex)
                        {
                            HandleException(ex, "Error while saving request");
                        }
                    }
                }
            }

            // For testing and debugging
            httpContext.Items["Palmalytics.Request.Tracked"] = saved;

            // Keep performance stats
            if (saved)
                performanceStats.Add((int)palmalyticsStopwatch.ElapsedMilliseconds);
        }

        private void HandleException(Exception exception, string message, params object[] args)
        {
            // Log the error
            logger.LogError(exception, message, args);

            // Keep track of successive errors
            Interlocked.Increment(ref successiveFailCount);

            // Stop tracking if too many errors in a row
            if (options.MaxErrorsBeforeFail > 0 && successiveFailCount > options.MaxErrorsBeforeFail)
            {
                logger.LogCritical("Too many errors, stopping tracking");
                options.EnableTracking = false;
            }

            if (options.ThrowExceptions)
                throw new PalmalyticsTrackingException(message, exception);
        }
    }
}