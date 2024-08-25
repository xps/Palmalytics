using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace Palmalytics
{
    public class PalmalyticsFilterOptions
    {
        public bool IgnorePalmalytics { get; set; } = true;
        public bool IgnoreAjax { get; set; } = true;
        public bool IgnoreStaticFiles { get; set; } = true;
        public bool IgnoreBots { get; set; } = true;

        public HashSet<string> IgnoreSegments { get; set; } = [];
        public HashSet<IPAddress> IgnoreIPAddresses { get; } = [];

        public HashSet<string> IncludeHttpVerbs { get; } = new(StringComparer.OrdinalIgnoreCase) { "GET" };
        public HashSet<string> IncludeContentTypes { get; } = new(StringComparer.OrdinalIgnoreCase) { "text/html" };
        public HashSet<int> IncludeStatusCodes { get; } = [200];

        public Func<HttpRequest, Func<HttpRequest, bool>, bool> CustomRequestFilter { get; set; }
        public Func<HttpResponse, Func<HttpResponse, bool>, bool> CustomResponseFilter { get; set; }
    }
}
