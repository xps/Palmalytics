using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Palmalytics.Services;
using static Palmalytics.Tests.TestHelpers.TestRequestsHelper;

namespace Palmalytics.Tests.Services
{
    public class RequestFilterTests
    {
        private readonly IServiceProvider serviceProvider;

        public RequestFilterTests(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [Fact]
        public void Test_RequestFilter_Default_Request_Passes_Default_Filter()
        {
            var options = new PalmalyticsFilterOptions();
            var filter = CreateRequestFilter(options);
            var request = CreateRequest();

            filter.ShouldTrackRequest(request).Should().BeTrue();
        }

        [Fact]
        public void Test_RequestFilter_IgnorePalmalytics_True()
        {
            var options = new PalmalyticsFilterOptions
            {
                IgnorePalmalytics = true
            };

            var filter = CreateRequestFilter(options);

            filter.ShouldTrackRequest(CreateRequest(path: "/test")).Should().BeTrue();
            filter.ShouldTrackRequest(CreateRequest(path: "/palmalytics")).Should().BeFalse();
            filter.ShouldTrackRequest(CreateRequest(path: "/palmalytics/")).Should().BeFalse();
            filter.ShouldTrackRequest(CreateRequest(path: "/palmalytics/api/data")).Should().BeFalse();
            filter.ShouldTrackRequest(CreateRequest(path: "/Palmalytics")).Should().BeFalse();
            filter.ShouldTrackRequest(CreateRequest(path: "/Palmalytics/")).Should().BeFalse();
            filter.ShouldTrackRequest(CreateRequest(path: "/Palmalytics/Api/Data")).Should().BeFalse();
        }

        [Fact]
        public void Test_RequestFilter_IgnorePalmalytics_False()
        {
            var options = new PalmalyticsFilterOptions
            {
                IgnorePalmalytics = false
            };

            var filter = CreateRequestFilter(options);

            filter.ShouldTrackRequest(CreateRequest(path: "/test")).Should().BeTrue();
            filter.ShouldTrackRequest(CreateRequest(path: "/palmalytics")).Should().BeTrue();
            filter.ShouldTrackRequest(CreateRequest(path: "/palmalytics/")).Should().BeTrue();
            filter.ShouldTrackRequest(CreateRequest(path: "/palmalytics/api/data")).Should().BeTrue();
            filter.ShouldTrackRequest(CreateRequest(path: "/Palmalytics")).Should().BeTrue();
            filter.ShouldTrackRequest(CreateRequest(path: "/Palmalytics/")).Should().BeTrue();
            filter.ShouldTrackRequest(CreateRequest(path: "/Palmalytics/Api/Data")).Should().BeTrue();
        }

        [Fact]
        public void Test_RequestFilter_IgnoreAjax_True()
        {
            var options = new PalmalyticsFilterOptions
            {
                IgnoreAjax = true
            };

            var filter = CreateRequestFilter(options);

            var normalRequest = CreateRequest();
            var ajaxRequest = CreateRequest(headers: new() { { "X-Requested-With", "XMLHttpRequest" } });

            filter.ShouldTrackRequest(normalRequest).Should().BeTrue();
            filter.ShouldTrackRequest(ajaxRequest).Should().BeFalse();
        }

        [Fact]
        public void Test_RequestFilter_IgnoreAjax_False()
        {
            var options = new PalmalyticsFilterOptions
            {
                IgnoreAjax = false
            };

            var filter = CreateRequestFilter(options);

            var normalRequest = CreateRequest();
            var ajaxRequest = CreateRequest(headers: new() { { "X-Requested-With", "XMLHttpRequest" } });

            filter.ShouldTrackRequest(normalRequest).Should().BeTrue();
            filter.ShouldTrackRequest(ajaxRequest).Should().BeTrue();
        }

        [Fact]
        public void Test_RequestFilter_IgnoreStaticFiles_True()
        {
            var options = new PalmalyticsFilterOptions
            {
                IgnoreStaticFiles = true
            };

            var filter = CreateRequestFilter(options);

            var normalRequest = CreateRequest();
            var staticFileRequest1 = CreateRequest(path: "/assets/main.js");
            var staticFileRequest2 = CreateRequest(path: "/assets/main.min.js");

            filter.ShouldTrackRequest(normalRequest).Should().BeTrue();
            filter.ShouldTrackRequest(staticFileRequest1).Should().BeFalse();
            filter.ShouldTrackRequest(staticFileRequest2).Should().BeFalse();
        }

        [Fact]
        public void Test_RequestFilter_IgnoreStaticFiles_False()
        {
            var options = new PalmalyticsFilterOptions
            {
                IgnoreStaticFiles = false
            };

            var filter = CreateRequestFilter(options);

            var normalRequest = CreateRequest();
            var staticFileRequest1 = CreateRequest(path: "/assets/main.js");
            var staticFileRequest2 = CreateRequest(path: "/assets/main.min.js");

            filter.ShouldTrackRequest(normalRequest).Should().BeTrue();
            filter.ShouldTrackRequest(staticFileRequest1).Should().BeTrue();
            filter.ShouldTrackRequest(staticFileRequest2).Should().BeTrue();
        }

        [Fact]
        public void Test_RequestFilter_IgnoreSegments()
        {
            var options = new PalmalyticsFilterOptions();
            options.IgnoreSegments.Add("/admin");
            options.IgnoreSegments.Add("/backoffice");

            var filter = CreateRequestFilter(options);

            filter.ShouldTrackRequest(CreateRequest(path: "/test")).Should().BeTrue();
            filter.ShouldTrackRequest(CreateRequest(path: "/admin")).Should().BeFalse();
            filter.ShouldTrackRequest(CreateRequest(path: "/admin/")).Should().BeFalse();
            filter.ShouldTrackRequest(CreateRequest(path: "/admin/test")).Should().BeFalse();
            filter.ShouldTrackRequest(CreateRequest(path: "/admin/test?param=true")).Should().BeFalse();
            filter.ShouldTrackRequest(CreateRequest(path: "/backoffice")).Should().BeFalse();
            filter.ShouldTrackRequest(CreateRequest(path: "/backoffice/")).Should().BeFalse();
            filter.ShouldTrackRequest(CreateRequest(path: "/backoffice/test")).Should().BeFalse();
            filter.ShouldTrackRequest(CreateRequest(path: "/backoffice/test?param=true")).Should().BeFalse();
        }

        [Fact]
        public void Test_RequestFilter_IgnoreSegments_IgnoresCase()
        {
            var options = new PalmalyticsFilterOptions();
            options.IgnoreSegments.Add("/admin");

            var filter = CreateRequestFilter(options);

            filter.ShouldTrackRequest(CreateRequest(path: "/Test")).Should().BeTrue();
            filter.ShouldTrackRequest(CreateRequest(path: "/Admin")).Should().BeFalse();
            filter.ShouldTrackRequest(CreateRequest(path: "/Admin/")).Should().BeFalse();
            filter.ShouldTrackRequest(CreateRequest(path: "/Admin/Test")).Should().BeFalse();
            filter.ShouldTrackRequest(CreateRequest(path: "/Admin/Test?Param=true")).Should().BeFalse();
        }

        [Fact]
        public void Test_RequestFilter_IgnoreIPAddresses_v4()
        {
            var options = new PalmalyticsFilterOptions();
            options.IgnoreIPAddresses.Add(IPAddress.Parse("1.2.3.4"));
            options.IgnoreIPAddresses.Add(IPAddress.Parse("5.6.7.8"));
            options.IgnoreIPAddresses.Add(IPAddress.Parse("5.6.7.8"));

            var filter = CreateRequestFilter(options);

            filter.ShouldTrackRequest(CreateRequest(ipAddress: "1.1.1.1")).Should().BeTrue();
            filter.ShouldTrackRequest(CreateRequest(ipAddress: "1.2.3.4")).Should().BeFalse();
            filter.ShouldTrackRequest(CreateRequest(ipAddress: "5.6.7.8")).Should().BeFalse();
        }

        [Fact]
        public void Test_RequestFilter_IgnoreIPAddresses_v4_Proxy()
        {
            var options = new PalmalyticsFilterOptions();
            options.IgnoreIPAddresses.Add(IPAddress.Parse("2.2.2.2"));

            var filter = CreateRequestFilter(options);

            var request1 = CreateRequest(ipAddress: "1.1.1.1", headers: new()
            {
                { "X-Forwarded-For", "2.2.2.2" }
            });

            var request2 = CreateRequest(ipAddress: "2.2.2.2", headers: new()
            {
                { "X-Forwarded-For", "1.1.1.1" }
            });

            filter.ShouldTrackRequest(request1).Should().BeFalse();
            filter.ShouldTrackRequest(request2).Should().BeTrue();
        }

        [Fact]
        public void Test_RequestFilter_IgnoreIPAddresses_v6()
        {
            var options = new PalmalyticsFilterOptions();
            options.IgnoreIPAddresses.Add(IPAddress.Parse("702c:8b07:d695:50d0:eacb:22a8:552b:a3f9"));
            options.IgnoreIPAddresses.Add(IPAddress.Parse("85b7:c8ec:7f99:ad0e:b46f:fadb:04e8:507d"));
            options.IgnoreIPAddresses.Add(IPAddress.Parse("85b7:c8ec:7f99:ad0e:b46f:fadb:04e8:507d"));

            var filter = CreateRequestFilter(options);

            filter.ShouldTrackRequest(CreateRequest(ipAddress: "7066:e495:6631:ae19:9fd0:2b92:2d25:597e")).Should().BeTrue();
            filter.ShouldTrackRequest(CreateRequest(ipAddress: "702c:8b07:d695:50d0:eacb:22a8:552b:a3f9")).Should().BeFalse();
            filter.ShouldTrackRequest(CreateRequest(ipAddress: "85b7:c8ec:7f99:ad0e:b46f:fadb:04e8:507d")).Should().BeFalse();
        }

        [Fact]
        public void Test_RequestFilter_IncludeHttpVerbs_Default()
        {
            var options = new PalmalyticsFilterOptions();
            var filter = CreateRequestFilter(options);

            var getRequest = CreateRequest(method: "GET");
            var postRequest = CreateRequest(method: "POST");
            var putRequest = CreateRequest(method: "PUT");
            var deleteRequest = CreateRequest(method: "DELETE");

            filter.ShouldTrackRequest(getRequest).Should().BeTrue();
            filter.ShouldTrackRequest(postRequest).Should().BeFalse();
            filter.ShouldTrackRequest(putRequest).Should().BeFalse();
            filter.ShouldTrackRequest(deleteRequest).Should().BeFalse();
        }

        [Fact]
        public void Test_RequestFilter_IncludeHttpVerbs_Custom()
        {
            var options = new PalmalyticsFilterOptions();
            options.IncludeHttpVerbs.Clear();
            options.IncludeHttpVerbs.Add("GET");
            options.IncludeHttpVerbs.Add("POST");

            var filter = CreateRequestFilter(options);

            var getRequest = CreateRequest(method: "GET");
            var postRequest = CreateRequest(method: "POST");
            var putRequest = CreateRequest(method: "PUT");
            var deleteRequest = CreateRequest(method: "DELETE");

            filter.ShouldTrackRequest(getRequest).Should().BeTrue();
            filter.ShouldTrackRequest(postRequest).Should().BeTrue();
            filter.ShouldTrackRequest(putRequest).Should().BeFalse();
            filter.ShouldTrackRequest(deleteRequest).Should().BeFalse();
        }

        [Fact]
        public void Test_RequestFilter_IncludeHttpVerbs_IgnoresCase()
        {
            var options = new PalmalyticsFilterOptions();
            options.IncludeHttpVerbs.Clear();
            options.IncludeHttpVerbs.Add("get");
            options.IncludeHttpVerbs.Add("post");

            var filter = CreateRequestFilter(options);

            var getRequest = CreateRequest(method: "GET");
            var postRequest = CreateRequest(method: "POST");
            var putRequest = CreateRequest(method: "PUT");
            var deleteRequest = CreateRequest(method: "DELETE");

            filter.ShouldTrackRequest(getRequest).Should().BeTrue();
            filter.ShouldTrackRequest(postRequest).Should().BeTrue();
            filter.ShouldTrackRequest(putRequest).Should().BeFalse();
            filter.ShouldTrackRequest(deleteRequest).Should().BeFalse();
        }

        [Fact]
        public void Test_RequestFilter_CustomFilter()
        {
            var options = new PalmalyticsFilterOptions
            {
                CustomRequestFilter = (request, defaultFilter) =>
                    request.Path.Value?.Length < 20
            };

            var filter = CreateRequestFilter(options);

            filter.ShouldTrackRequest(CreateRequest(path: "/some/path")).Should().BeTrue();
            filter.ShouldTrackRequest(CreateRequest(path: "/some/path.js")).Should().BeTrue();
            filter.ShouldTrackRequest(CreateRequest(path: "/some/long/long/long/long/path")).Should().BeFalse();
            filter.ShouldTrackRequest(CreateRequest(path: "/some/long/long/long/long/path.js")).Should().BeFalse();
        }

        [Fact]
        public void Test_RequestFilter_CustomFilter_Calling_DefaultFilter()
        {
            var options = new PalmalyticsFilterOptions
            {
                IgnoreStaticFiles = false,
                CustomRequestFilter = (request, defaultFilter) =>
                    request.Path.Value?.Length < 20 || defaultFilter(request)
            };

            var filter = CreateRequestFilter(options);

            filter.ShouldTrackRequest(CreateRequest(path: "/some/path")).Should().BeTrue();
            filter.ShouldTrackRequest(CreateRequest(path: "/some/path.js")).Should().BeTrue();
            filter.ShouldTrackRequest(CreateRequest(path: "/some/long/long/long/long/path")).Should().BeTrue();
            filter.ShouldTrackRequest(CreateRequest(path: "/some/long/long/long/long/path.js")).Should().BeTrue();
            filter.ShouldTrackRequest(CreateRequest(path: "/some/long/long/long/long/path", method: "POST")).Should().BeFalse();
        }

        [Fact]
        public void Test_RequestFilter_Ignores_Prefetches()
        {
            var options = new PalmalyticsFilterOptions();
            var filter = CreateRequestFilter(options);

            var request = CreateRequest(headers: new()
            {
                { "Sec-Purpose", "prefetch" }
            });

            filter.ShouldTrackRequest(request).Should().BeFalse();
        }

        private RequestFilter CreateRequestFilter(PalmalyticsFilterOptions options)
        {
            return ActivatorUtilities.CreateInstance<RequestFilter>(serviceProvider, Options.Create(options));
        }
    }
}
