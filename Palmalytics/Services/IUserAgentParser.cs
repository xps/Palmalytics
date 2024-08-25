using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Palmalytics.Model;

namespace Palmalytics.Services
{
    public interface IUserAgentParser
    {
        Device GetDevice(HttpRequest request);
        Device GetDevice(string userAgent, Dictionary<string, string> headers = null);
    }
}