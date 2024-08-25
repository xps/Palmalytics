using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Palmalytics.Dashboard
{
    public interface IDashboardRequestHandler
    {
        Task HandleRequestAsync(HttpContext context);
    }
}
