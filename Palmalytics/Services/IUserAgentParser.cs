using Microsoft.AspNetCore.Http;
using Palmalytics.Model;

namespace Palmalytics.Services
{
    public interface IUserAgentParser
    {
        Device GetDevice(HttpRequest request);
    }
}