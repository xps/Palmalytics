using Microsoft.AspNetCore.Http;
using Palmalytics.Extensions;

namespace Palmalytics
{
    public static class PalmalyticsAuthorization
    {
        public static bool AllowLocalRequestsOnly(HttpContext context)
        {
            return context.Request.IsLocal();
        }
    }
}
