using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Palmalytics.Dashboard
{
    public class DashboardRequestHandler : RequestHandlerBase, IDashboardRequestHandler
    {
        public DashboardRequestHandler(IOptions<PalmalyticsOptions> options, ILogger<DashboardRequestHandler> logger) : base(options, logger)
        {
        }

        public override Result GetResultForRequest(HttpRequest request)
        {
            // We only support GET requests
            if (!HttpMethods.IsGet(request.Method))
                return StatusCode(405, "Method not allowed");

            var path = request.Path.Value;

            try
            {
                if (path is "" or "/")
                    return Index();

                return File(path);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, @"Error handling request for {url}", request.Path + request.QueryString);
                if (options.DashboardOptions.ShowExceptionDetails)
                    return StatusCode(500, ex.ToString());
                else
                    return StatusCode(500, "Internal server error. Set `options.Dashboard.ShowExceptionDetails = true` to see the details.");
            }
        }

        protected virtual string GetResourceNameForPath(string path)
        {
            return "Palmalytics.Dashboard.WebRoot" + path.Replace('/', '.');
        }

        protected virtual Stream GetResourceForPath(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = GetResourceNameForPath(path);
            return assembly.GetManifestResourceStream(resourceName);
        }

        protected virtual Result Index()
        {
            // Check that the front-end has been built and included as embedded resources
            var assembly = Assembly.GetExecutingAssembly();
            if (!assembly.GetManifestResourceNames().Any())
            {
                return NotFound("Front-end code not found. You need to build it.");
            }

            // Get the index.html file
            var path = "/index.html";
            using var inputStream = GetResourceForPath(path);
            if (inputStream == null)
            {
                return NotFound($"Can't find embedded resource with name: {GetResourceForPath(path)}");
            }

            // Write the response
            var html = new StreamReader(inputStream).ReadToEnd();
            return Content(html, "text/html");
        }

        protected virtual Result File(string path)
        {
            using var inputStream = GetResourceForPath(path);

            if (inputStream == null)
                return NotFound($"Can't find embedded resource with name: {GetResourceNameForPath(path)}");

            // Read the stream to a byte array
            // That's not so efficient, but it probably doesn't matter
            using var memoryStream = new MemoryStream();
            inputStream.CopyTo(memoryStream);
            var bytes = memoryStream.ToArray();

            return new BytesResult
            {
                Code = 200,
                ContentType = GetContentType(path),
                Body = bytes
            };
        }

        protected virtual string GetContentType(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();

            return extension switch
            {
                ".htm" or ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".svg" => "image/svg+xml",
                _ => throw new Exception($"Unknown file extension: {extension}")
            };
        }
    }
}
