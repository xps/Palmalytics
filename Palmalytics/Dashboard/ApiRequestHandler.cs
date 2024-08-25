using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Palmalytics.Extensions;
using Palmalytics.Middleware;
using Palmalytics.Model;
using Palmalytics.Utilities;

namespace Palmalytics.Dashboard
{
    internal class ApiRequestHandler : RequestHandlerBase, IApiRequestHandler
    {
        private readonly IDataStore dataStore;

        public ApiRequestHandler(IDataStore dataStore, IOptions<PalmalyticsOptions> options, ILogger<ApiRequestHandler> logger) : base(options, logger)
        {
            this.dataStore = dataStore;
        }

        public override Result GetResultForRequest(HttpRequest request)
        {
            // Map the request to a method
            var method = MapRequestToMethod(request);
            if (method == null)
            {
                return NotFound($"Cannot find method for {request.Method} {request.Path}");
            }

            // Map the query string to method parameters
            List<object> args;
            try
            {
                args = MapRequestToArguments(request, method);
            }
            catch (FormatException ex)
            {
                return BadRequest(ex.Message);
            }

            // Call the method to get the result
            var result = method.Invoke(this, args.ToArray());

            // Check that the result is valid
            if (result == null && result is not Result)
            {
                throw new Exception($"Invalid return type for API endpoint. Expected type {typeof(Result)}, but got {result.GetType()}.");
            }

            return (Result)result;
        }

        protected virtual MethodInfo MapRequestToMethod(HttpRequest request)
        {
            // We'll map GET /some-data to GetSomeData()
            var methodName = request.Method.Capitalize() + request.Path.Value.TrimStart('/').ConvertKebabToPascalCase();
            var method = GetType().GetMethod(methodName);
            return method;
        }

        protected virtual List<object> MapRequestToArguments(HttpRequest request, MethodInfo method)
        {
            var parameters = method.GetParameters();
            var args = new List<object>();

            foreach (var parameter in parameters)
            {
                var value = GetArgumentValue(request, parameter.Name, parameter.ParameterType, parameter.DefaultValue);
                args.Add(value);
            }

            return args;
        }

        // TODO: could we be using ASP.NET Core's model binding instead?
        protected object GetArgumentValue(HttpRequest request, string name, Type type, object defaultValue = null)
        {
            // Special case for complex types
            if (type.IsClass && type != typeof(string))
            {
                // If the argument is a class, read values for each of the properties
                var obj = Activator.CreateInstance(type);
                foreach (var property in type.GetProperties())
                {
                    var propertyValue = GetArgumentValue(request, property.Name, property.PropertyType, DBNull.Value);
                    property.SetValue(obj, propertyValue);
                }
                return obj;
            }

            // Read the value from the query string
            var value = (object)request.Query[name].FirstOrDefault();
            if (value == null)
            {
                // If there's no value in the query string, use the supplied default value, or the type's default value, or null
                if (defaultValue is not DBNull)
                    value = defaultValue;
                else if (type.IsValueType)
                    value = Activator.CreateInstance(type);
                else
                    value = null;
            }

            // For nullables, take the underlying type
            var targetType = Nullable.GetUnderlyingType(type) ?? type;
            if (value != null)
            {
                // If we have a non-null value, convert it to the target type
                if (targetType.IsEnum)
                    return Enum.Parse(targetType, value.ToString(), ignoreCase: true);
                else
                    return Convert.ChangeType(value, targetType);
            }

            return value;
        }

        public Result GetVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fullVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            var version = fullVersion.Split('+')[0];

            return Json(new
            {
                version
            });
        }

        public Result GetPerformanceStats()
        {
            var timings = TrackingMiddleware.PerformanceStats.Reverse();

            if (!timings.Any())
                return Json(new { mean = 0, average = 0, timings });

            var stats = new
            {
                mean = timings.OrderBy(x => x).ElementAt(timings.Count() / 2),
                average = timings.Average(),
                timings
            };

            return Json(stats);
        }

        public Result GetTopData(string period, Filters filters)
        {
            var dates = DateHelpers.GetDateRangeForPeriod(period);
            var chartData = dataStore.GetTopData(dates.DateFrom, dates.DateTo, filters);

            return Json(chartData);
        }

        public Result GetChart(string period, Interval interval, string property, Filters filters)
        {
            var dates = DateHelpers.GetDateRangeForPeriod(period);
            var chartData = dataStore.GetChart(dates.DateFrom, dates.DateTo, interval, property, filters);

            return Json(chartData);
        }

        public Result GetBrowsers(string period, Filters filters, int page = 1)
        {
            var dates = DateHelpers.GetDateRangeForPeriod(period);
            var chartData = dataStore.GetBrowsers(dates.DateFrom, dates.DateTo, filters, page);
            return Json(chartData);
        }

        public Result GetOperatingSystems(string period, Filters filters, int page = 1)
        {
            var dates = DateHelpers.GetDateRangeForPeriod(period);
            var chartData = dataStore.GetOperatingSystems(dates.DateFrom, dates.DateTo, filters, page);
            return Json(chartData);
        }

        public Result GetReferrers(string period, Filters filters, int page = 1)
        {
            var dates = DateHelpers.GetDateRangeForPeriod(period);
            var chartData = dataStore.GetReferrers(dates.DateFrom, dates.DateTo, filters, page);
            return Json(chartData);
        }

        public Result GetCountries(string period, Filters filters, int page = 1)
        {
            var dates = DateHelpers.GetDateRangeForPeriod(period);
            var chartData = dataStore.GetCountries(dates.DateFrom, dates.DateTo, filters, page);
            return Json(chartData);
        }

        public Result GetTopPages(string period, Filters filters, int page = 1)
        {
            var dates = DateHelpers.GetDateRangeForPeriod(period);
            var chartData = dataStore.GetTopPages(dates.DateFrom, dates.DateTo, filters, page);
            return Json(chartData);
        }

        public Result GetEntryPages(string period, Filters filters, int page = 1)
        {
            var dates = DateHelpers.GetDateRangeForPeriod(period);
            var chartData = dataStore.GetEntryPages(dates.DateFrom, dates.DateTo, filters, page);
            return Json(chartData);
        }

        public Result GetExitPages(string period, Filters filters, int page = 1)
        {
            var dates = DateHelpers.GetDateRangeForPeriod(period);
            var chartData = dataStore.GetExitPages(dates.DateFrom, dates.DateTo, filters, page);
            return Json(chartData);
        }

        // Not used for now
        // public Result GetLastRequests()
        // {
        //     var requests = dataStore.GetLastRequests();
        //     return Json(new { requests });
        // }
    }
}
