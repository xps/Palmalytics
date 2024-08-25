using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Palmalytics.Dashboard
{
    public interface IApiRequestHandler
    {
        Task HandleRequestAsync(HttpContext context);
    }
}
