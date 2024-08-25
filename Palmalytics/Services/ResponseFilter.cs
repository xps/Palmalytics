using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Palmalytics.Services
{
    public class ResponseFilter(IOptions<PalmalyticsFilterOptions> options) : IResponseFilter
    {
        private readonly PalmalyticsFilterOptions options = options.Value;

        public PalmalyticsFilterOptions Options => options;

        public virtual bool ShouldTrackResponse(HttpResponse response)
        {
            if (options.CustomResponseFilter != null)
            {
                return options.CustomResponseFilter(response, ShouldTrackResponse_ExceptCustomFilter);
            }

            return ShouldTrackResponse_ExceptCustomFilter(response);
        }

        private bool ShouldTrackResponse_ExceptCustomFilter(HttpResponse response)
        {
            if (options.IncludeStatusCodes.Any())
            {
                if (!options.IncludeStatusCodes.Contains(response.StatusCode))
                    return false;
            }

            if (options.IncludeContentTypes.Any())
            {
                var contentType = response.ContentType?.Split(';').FirstOrDefault();
                if (!options.IncludeContentTypes.Contains(contentType))
                    return false;
            }

            return true;
        }
    }
}
