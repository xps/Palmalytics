using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Palmalytics.Model;

namespace Palmalytics
{
    public class PalmalyticsOptions
    {
        public bool EnableTracking { get; set; } = true;

        // TODO: add option to enable dashboard or not
        // public bool EnableDashboard { get; set; } = true;

        public PalmalyticsDashboardOptions DashboardOptions { get; } = new();
        public PalmalyticsFilterOptions FilterOptions { get; } = new();
        public PalmalyticsParserOptions ParserOptions { get; } = new();

        public int MaxErrorsBeforeFail { get; set; } = 20;

        public Type DataStoreType { get; private set; }
        public object DataStoreOptions { get; set; }

        public bool ThrowExceptions { get; set; } = false;
        public bool AutomaticallyDownloadGeocodingData { get; set; } = true;
        public bool EnableAsyncWrites { get; set; } = true;

        public void UseDataStore<T>(object options = null) where T : IDataStore
        {
            DataStoreType = typeof(T);
            DataStoreOptions = options;
        }

        public void UseDataStore(Type type, object options = null)
        {
            // Check that type implements IDataStore
            if (!typeof(IDataStore).IsAssignableFrom(type))
                throw new ArgumentException($"Type {type} must implement IDataStore", nameof(type));

            DataStoreType = type;
            DataStoreOptions = options;
        }

        // Events
        public EventHandler<OnParseRequestEventArgs> OnParseRequest { get; set; }
        public EventHandler<OnBeforeSavingRequestEventArgs> OnBeforeSavingRequest { get; set; }

        // Overrideable method to get the client IP address
        public Func<HttpRequest, IPAddress> GetClientIPAddress { get; set; } = (request) =>
        {
            // If behind Cloudflare, use the CF-Connecting-IPv6 or CF-Connecting-IP header
            if (request.Headers.ContainsKey("CF-Connecting-IPv6"))
                return IPAddress.Parse(request.Headers["CF-Connecting-IPv6"].ToString());
            if (request.Headers.ContainsKey("CF-Connecting-IP"))
                return IPAddress.Parse(request.Headers["CF-Connecting-IP"].ToString());

            // If behind another proxy, use the X-Forwarded-For header
            if (request.Headers.ContainsKey("X-Forwarded-For"))
                return IPAddress.Parse(request.Headers["X-Forwarded-For"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries)[0].Trim());

            return request.HttpContext.Connection.RemoteIpAddress;
        };
    }

    public class OnParseRequestEventArgs(HttpContext context, RequestData requestData) : EventArgs
    {
        public HttpContext HttpContext { get; } = context;
        public RequestData RequestData { get; } = requestData;
    }

    public class OnBeforeSavingRequestEventArgs(HttpContext context, RequestData requestData) : EventArgs
    {
        public HttpContext HttpContext { get; } = context;
        public RequestData RequestData { get; } = requestData;
    }
}
