using Microsoft.AspNetCore.Http;

namespace Palmalytics.Services
{
    public interface IResponseFilter
    {
        bool ShouldTrackResponse(HttpResponse response);
    }
}