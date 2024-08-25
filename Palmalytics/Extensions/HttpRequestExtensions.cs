using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Palmalytics.Extensions
{
    public static class HttpRequestExtensions
    {
        public static bool IsLocal(this HttpRequest request)
        {
            var connection = request.HttpContext.Connection;

            if (connection.RemoteIpAddress != null)
            {
                if (IPAddress.IsLoopback(connection.RemoteIpAddress))
                    return true;

                if (connection.LocalIpAddress != null)
                    return connection.RemoteIpAddress.Equals(connection.LocalIpAddress);
            }

            return false;
        }

        internal static string GetDebugString(this HttpRequest request)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{request.Method} {request.Path}{request.QueryString}");
            foreach (var header in request.Headers)
                sb.AppendLine($"{header.Key}: {header.Value}");

            return sb.ToString().TrimEnd();
        }
    }
}
