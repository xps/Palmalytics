using System;
using Microsoft.AspNetCore.Http;

namespace Palmalytics
{
    public class PalmalyticsDashboardOptions
    {
        public Func<HttpContext, bool> Authorize { get; set; } = PalmalyticsAuthorization.AllowLocalRequestsOnly;
        public bool ShowExceptionDetails { get; set; } = false;
    }
}
