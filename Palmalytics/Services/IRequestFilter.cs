using Microsoft.AspNetCore.Http;

namespace Palmalytics.Services
{
    public interface IRequestFilter
    {
        bool ShouldTrackRequest(HttpRequest request);
    }
}