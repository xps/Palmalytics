using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Palmalytics.Dashboard
{
    public abstract class RequestHandlerBase(IOptions<PalmalyticsOptions> options, ILogger logger)
    {
        protected readonly ILogger logger = logger;
        protected readonly PalmalyticsOptions options = options.Value;

        public virtual Task HandleRequestAsync(HttpContext context)
        {
            try
            {
                // Authorization
                if (options.DashboardOptions.Authorize != null && !options.DashboardOptions.Authorize(context))
                    return WriteResponseAsync(context, StatusCode(403, "Forbidden. Set authorization rules with `options.DashboardOptions.Authorize = <authorization_method>`."));

                // Get the result
                var result = GetResultForRequest(context.Request);

                // Write the result to the response
                if (result != null)
                    return WriteResponseAsync(context, result);
                else
                    return WriteResponseAsync(context, NoContent());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, @"Error handling request for {url}", context.Request.Path + context.Request.QueryString);
                return WriteResponseAsync(context, InternalError(ex));
            }
        }

        public abstract Result GetResultForRequest(HttpRequest request);

        protected virtual async Task WriteResponseAsync(HttpContext context, Result result)
        {
            await result.WriteResponseAsync(context.Response);
        }

        protected virtual Result StatusCode(int statusCode, string message = null)
        {
            return new StringResult
            {
                Code = statusCode,
                Body = message,
                ContentType = "text/plain"
            };
        }

        protected virtual Result Content(string content, string contentType)
        {
            return new StringResult
            {
                Code = 200,
                Body = content,
                ContentType = contentType
            };
        }

        protected virtual Result Content(byte[] content, string contentType)
        {
            return new BytesResult
            {
                Code = 200,
                Body = content,
                ContentType = contentType
            };
        }

        protected virtual Result Json(object data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });

            return new StringResult
            {
                Code = 200,
                Body = json,
                ContentType = "application/json"
            };
        }

        protected virtual Result NotFound(string message)
        {
            return new StringResult
            {
                Code = 404,
                Body = message,
                ContentType = "text/plain"
            };
        }

        protected virtual Result BadRequest(string message)
        {
            return new StringResult
            {
                Code = 400,
                Body = message,
                ContentType = "text/plain"
            };
        }

        protected virtual Result NoContent()
        {
            return new Result
            {
                Code = 204
            };
        }

        protected virtual Result InternalError(Exception exception)
        {
            if (!options.DashboardOptions.ShowExceptionDetails)
                return StatusCode(500, "Internal server error. Set `options.Dashboard.ShowExceptionDetails = true` to see the details.");

            var messageBuilder = new StringBuilder(exception.ToString());

            var data = new List<DictionaryEntry>();

            do
            {
                foreach (var entry in exception.Data.Cast<DictionaryEntry>())
                    data.Add(entry);

            } while ((exception = exception.InnerException) != null);

            if (data.Any())
            {
                messageBuilder.AppendLine();
                messageBuilder.AppendLine("---");
                messageBuilder.AppendLine("Additional data:");

                foreach (var entry in data)
                {
                    messageBuilder.AppendFormat("{0} = {1}", entry.Key, entry.Value);
                    messageBuilder.AppendLine();
                }
            }

            return StatusCode(500, messageBuilder.ToString());
        }

        public class Result
        {
            public int Code { get; set; }
            public string ContentType { get; set; }

            public virtual Task WriteResponseAsync(HttpResponse response)
            {
                response.StatusCode = Code;
                response.ContentType = ContentType;
                return Task.CompletedTask;
            }
        }

        public class StringResult : Result
        {
            public string Body { get; set; }

            public override async Task WriteResponseAsync(HttpResponse response)
            {
                await base.WriteResponseAsync(response);
                if (Body != null)
                    await response.WriteAsync(Body);
            }
        }

        public class BytesResult : Result
        {
            public byte[] Body { get; set; }

            public override async Task WriteResponseAsync(HttpResponse response)
            {
                await base.WriteResponseAsync(response);
                if (Body != null)
                    await response.Body.WriteAsync(Body);
            }
        }
    }
}
