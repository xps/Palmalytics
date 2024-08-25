using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Palmalytics.Services
{
    public class RequestFilter : IRequestFilter
    {
        private readonly PalmalyticsOptions topLevelOptions;
        private readonly PalmalyticsFilterOptions options;

        public PalmalyticsOptions TopLevelOptions => topLevelOptions;
        public PalmalyticsFilterOptions Options => options;

        public RequestFilter(IOptions<PalmalyticsOptions> topLevelOptions, IOptions<PalmalyticsFilterOptions> options)
        {
            this.topLevelOptions = topLevelOptions.Value;
            this.options = options.Value;
        }

        public virtual bool ShouldTrackRequest(HttpRequest request)
        {
            if (request.Headers.ContainsKey("Sec-Purpose"))
            {
                var purpose = request.Headers["Sec-Purpose"].ToString();
                if (purpose.Contains("prefetch", StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            if (options.CustomRequestFilter != null)
            {
                return options.CustomRequestFilter(request, ShouldTrackRequest_ExceptCustomFilter);
            }

            return ShouldTrackRequest_ExceptCustomFilter(request);
        }

        private bool ShouldTrackRequest_ExceptCustomFilter(HttpRequest request)
        {
            if (options.IgnorePalmalytics && request.Path.StartsWithSegments("/palmalytics"))
            {
                return false;
            }

            if (options.IgnoreAjax && request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return false;
            }

            if (options.IgnoreStaticFiles && !string.IsNullOrWhiteSpace(Path.GetExtension(request.Path)))
            {
                return false;
            }

            if (options.IgnoreSegments.Any())
            {
                if (options.IgnoreSegments.Any(x => request.Path.StartsWithSegments(x)))
                    return false;
            }

            if (options.IgnoreIPAddresses.Any())
            {
                var ipAddress = topLevelOptions.GetClientIPAddress(request);
                if (options.IgnoreIPAddresses.Contains(ipAddress))
                    return false;
            }

            if (options.IncludeHttpVerbs.Any())
            {
                if (!options.IncludeHttpVerbs.Contains(request.Method))
                    return false;
            }

            return true;
        }
    }
}
