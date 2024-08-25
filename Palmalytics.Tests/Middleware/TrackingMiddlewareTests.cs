using HttpContextMoq;
using HttpContextMoq.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Palmalytics.Exceptions;
using Palmalytics.Middleware;
using Palmalytics.Tests.TestHelpers;

namespace Palmalytics.Tests.Middleware
{
    public class TrackingMiddlewareTests
    {
        // Some tests that involve static resources cannot run in parallel
        // TODO: use [ThreadStatic] instead?
        private readonly static SemaphoreSlim semaphore = new(1, 1);

        [Fact]
        public async Task Test_TrackingMiddleware_Enabled()
        {
            var next = CreateNextRequestDelegate();

            var middleware = CreateTrackingMiddleware(x =>
            {
                x.EnableTracking = true;
            });

            var request = TestRequestsHelper.CreateRequest(path: "/test");

            await middleware.InvokeAsync(request.HttpContext, next);

            var requests = middleware.DataStore.GetLastRequests();
            requests.Should().HaveCount(1);
            requests[0].Path.Should().Be("/test");
        }

        [Fact]
        public async Task Test_TrackingMiddleware_Disabled()
        {
            var next = CreateNextRequestDelegate();

            var middleware = CreateTrackingMiddleware(x =>
            {
                x.EnableTracking = false;
            });

            var request = TestRequestsHelper.CreateRequest(path: "/test");

            await middleware.InvokeAsync(request.HttpContext, next);

            var requests = middleware.DataStore.GetLastRequests();
            requests.Should().BeEmpty();
        }

        [Fact]
        public async Task Test_TrackingMiddleware_ThrowsExceptions_False()
        {
            var next = CreateNextRequestDelegate();

            var middleware = CreateTrackingMiddleware(x =>
            {
                x.ParserOptions.CollectIPAddress = true;
                x.ThrowExceptions = false;
            });

            // A null IP address will cause an exception
            var request = TestRequestsHelper.CreateRequest(ipAddress: null);

            await middleware.InvokeAsync(request.HttpContext, next);

            var requests = middleware.DataStore.GetLastRequests();
            requests.Should().BeEmpty();

            // TODO: check for an error log
        }

        [Fact]
        public async Task Test_TrackingMiddleware_ThrowsExceptions_True()
        {
            var next = CreateNextRequestDelegate();

            var middleware = CreateTrackingMiddleware(x =>
            {
                x.ParserOptions.CollectIPAddress = true;
                x.ThrowExceptions = true;
            });

            // A null IP address will cause an exception
            var request = TestRequestsHelper.CreateRequest(ipAddress: null);

            (await middleware.Invoking(x => x.InvokeAsync(request.HttpContext, next))
                .Should().ThrowAsync<PalmalyticsTrackingException>()
                .WithMessage("Error in request parser"))
                .WithInnerException<NullReferenceException>();

            // TODO: check for an error log
        }

        [Fact]
        public async Task Test_TrackingMiddleware_Uses_RequestFilters()
        {
            var next = CreateNextRequestDelegate();

            var middleware = CreateTrackingMiddleware(x =>
            {
                x.FilterOptions.IgnoreSegments.Add("/ignore");
            });

            var request1 = TestRequestsHelper.CreateRequest(path: "/test");
            var request2 = TestRequestsHelper.CreateRequest(path: "/ignore");

            await middleware.InvokeAsync(request1.HttpContext, next);
            await middleware.InvokeAsync(request2.HttpContext, next);

            var requests = middleware.DataStore.GetLastRequests();
            requests.Should().HaveCount(1);
            requests[0].Path.Should().Be("/test");
        }

        [Fact]
        public async Task Test_TrackingMiddleware_Uses_ResponseFilters()
        {
            var next = CreateNextRequestDelegate(statusCode: 500);
            var middleware = CreateTrackingMiddleware();
            var request = TestRequestsHelper.CreateRequest();

            await middleware.InvokeAsync(request.HttpContext, next);

            var requests = middleware.DataStore.GetLastRequests();
            requests.Should().BeEmpty();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        // TODO: all parameter tests on this model?
        public async Task Test_TrackingMiddleware_IgnoresBots(bool ignoreBots)
        {
            var next = CreateNextRequestDelegate();

            var middleware = CreateTrackingMiddleware(x =>
            {
                x.FilterOptions.IgnoreBots = ignoreBots;
            });

            var normalRequest = TestRequestsHelper.CreateRequest(path: "/path-1");
            var botRequest = TestRequestsHelper.CreateRequest(path: "/path-2", userAgent: "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)");

            await middleware.InvokeAsync(normalRequest.HttpContext, next);
            await middleware.InvokeAsync(botRequest.HttpContext, next);

            normalRequest.HttpContext.Items["Palmalytics.Request.Tracked"].Should().Be(true);
            botRequest.HttpContext.Items["Palmalytics.Request.Tracked"].Should().Be(!ignoreBots);
        }

        [Fact]
        public async Task Test_TrackingMiddleware_IgnoresNullUserAgents()
        {
            var next = CreateNextRequestDelegate();

            var middleware = CreateTrackingMiddleware(x =>
            {
                x.FilterOptions.IgnoreBots = true;
            });

            var requests1 = TestRequestsHelper.CreateRequest(userAgent: null);
            var requests2 = TestRequestsHelper.CreateRequest(userAgent: "");

            await middleware.InvokeAsync(requests1.HttpContext, next);
            await middleware.InvokeAsync(requests2.HttpContext, next);

            requests1.HttpContext.Items["Palmalytics.Request.Tracked"].Should().Be(false);
            requests2.HttpContext.Items["Palmalytics.Request.Tracked"].Should().Be(false);
        }

        [Fact]
        public async Task Test_TrackingMiddleware_Collects_Response_Data()
        {
            var next = CreateNextRequestDelegate();

            var middleware = CreateTrackingMiddleware();

            var request = TestRequestsHelper.CreateRequest();

            await middleware.InvokeAsync(request.HttpContext, next);

            var requests = middleware.DataStore.GetLastRequests();
            requests.Should().HaveCount(1);
            requests[0].ResponseTime.Should().BeGreaterThan(0);
            requests[0].ResponseCode.Should().Be(200);
            requests[0].ContentType.Should().Be("text/html");
        }

        [Fact(Skip = "Doesn't work when other tests run in parallel")]
        public async Task Test_TrackingMiddleware_Collects_Perf_Stats()
        {
            var next = CreateNextRequestDelegate();

            var middleware = CreateTrackingMiddleware(options =>
            {
                options.FilterOptions.IgnoreSegments.Add("/ignore");
            });

            await semaphore.WaitAsync();
            try
            {
                var initialCount = TrackingMiddleware.PerformanceStats.Length;

                await middleware.InvokeAsync(TestRequestsHelper.CreateRequest(path: "/test").HttpContext, next);
                await middleware.InvokeAsync(TestRequestsHelper.CreateRequest(path: "/ignore").HttpContext, next);
                await middleware.InvokeAsync(TestRequestsHelper.CreateRequest(path: "/test").HttpContext, next);

                TrackingMiddleware.PerformanceStats.Should().HaveCount(initialCount + 2);
            }
            finally
            {
                semaphore.Release();
            }
        }

        [Fact]
        public async Task Test_TrackingMiddleware_Disables_When_Fails_Many_Times()
        {
            var next = CreateNextRequestDelegate();

            var middleware = CreateTrackingMiddleware(options =>
            {
                options.EnableTracking = true;
                options.MaxErrorsBeforeFail = 5;
            });

            for (var i = 0; i <= 5; i++)
            {
                await middleware.InvokeAsync(TestRequestsHelper.CreateRequest(ipAddress: null).HttpContext, next);
            }

            middleware.Options.EnableTracking.Should().BeFalse();
        }

        [Fact]
        public async Task Test_TrackingMiddleware_OnBeforeSaveEvent_Aggregate_Urls()
        {
            var next = CreateNextRequestDelegate();

            var middleware = CreateTrackingMiddleware(options =>
            {
                options.OnParseRequest = (_, e) =>
                {
                    if (e.RequestData.Path.StartsWith("/session/", StringComparison.OrdinalIgnoreCase))
                        e.RequestData.Path = "/session/*";
                };
            });

            var request = TestRequestsHelper.CreateRequest(path: "/Session/abc/def");

            await middleware.InvokeAsync(request.HttpContext, next);

            var requests = middleware.DataStore.GetLastRequests();
            requests.Should().HaveCount(1);
            requests[0].Path.Should().Be("/session/*");
        }

        [Fact]
        public async Task Test_RequestParser_BeforeLogRequest_IncludeQueryStringParameter()
        {
            var next = CreateNextRequestDelegate();

            var middleware = CreateTrackingMiddleware(options =>
            {
                options.ParserOptions.CollectQueryString = false;
                options.OnParseRequest = (_, e) =>
                {
                    if (string.Equals(e.RequestData.Path, "/blog-post", StringComparison.OrdinalIgnoreCase))
                        e.RequestData.Path = "/blog-post?id=" + e.HttpContext.Request.Query["id"];
                };
            });

            var request = TestRequestsHelper.CreateRequest(path: "/Blog-Post?Id=123&Session=xyz");

            await middleware.InvokeAsync(request.HttpContext, next);

            var requests = middleware.DataStore.GetLastRequests();
            requests.Should().HaveCount(1);
            requests[0].Path.Should().Be("/blog-post?id=123");
        }

        [Fact]
        public async Task Test_TrackingMiddleware_OnBeforeSaveEvent()
        {
            var next = CreateNextRequestDelegate();

            var middleware = CreateTrackingMiddleware(options =>
            {
                options.OnBeforeSavingRequest = (_, e) =>
                {
                    e.RequestData.Path.Should().Be("/test");

                    e.RequestData.UserName = "john.doe";
                    e.RequestData.CustomData = "xyz";
                };
            });

            var request = TestRequestsHelper.CreateRequest(path: "/test");

            await middleware.InvokeAsync(request.HttpContext, next);

            var requests = middleware.DataStore.GetLastRequests();
            requests.Should().HaveCount(1);
            requests[0].UserName.Should().Be("john.doe");
            requests[0].CustomData.Should().Be("xyz");
        }

        [Fact]
        public async Task Test_TrackingMiddleware_OnBeforeSaveEvent_Exception()
        {
            var next = CreateNextRequestDelegate();

            var middleware = CreateTrackingMiddleware(options =>
            {
                options.ThrowExceptions = true;
                options.OnBeforeSavingRequest = (_, e) =>
                {
                    throw new Exception("Test");
                };
            });

            var request = TestRequestsHelper.CreateRequest(path: "/test");

            (await middleware.Invoking(x => x.InvokeAsync(request.HttpContext, next))
                .Should().ThrowAsync<PalmalyticsTrackingException>()
                .WithMessage("Error in OnBeforeSavingRequest event handler"))
                .WithInnerException<Exception>()
                .WithMessage("Test");

            // TODO: check for an error log
        }

        [Fact]
        public async Task Test_TrackingMiddleware_FailCount_Resets()
        {
            var next = CreateNextRequestDelegate();

            var middleware = CreateTrackingMiddleware(options =>
            {
                options.EnableTracking = true;
                options.MaxErrorsBeforeFail = 100;
                options.EnableAsyncWrites = false;
            });

            await semaphore.WaitAsync();
            try
            {
                for (var i = 0; i < 3; i++)
                {
                    // The null IP address should cause an exception
                    await middleware.InvokeAsync(TestRequestsHelper.CreateRequest(ipAddress: null).HttpContext, next);
                }

                TrackingMiddleware.SuccessiveFailCount.Should().BeGreaterThan(0);

                await middleware.InvokeAsync(TestRequestsHelper.CreateRequest().HttpContext, next);

                TrackingMiddleware.SuccessiveFailCount.Should().Be(0);
            }
            finally
            {
                semaphore.Release();
            }
        }

        [Fact]
        public async Task Test_TrackingMiddleware_Logs_Errors()
        {
            var next = CreateNextRequestDelegate();

            var middleware = CreateTrackingMiddleware();

            await semaphore.WaitAsync();
            try
            {
                var initialCount = MemoryLogger.Logs.Count;

                for (var i = 0; i < 3; i++)
                {
                    await middleware.InvokeAsync(TestRequestsHelper.CreateRequest(ipAddress: null).HttpContext, next);
                }

                MemoryLogger.Logs.Should().HaveCount(initialCount + 3);
                MemoryLogger.Logs.Last().LogLevel.Should().Be(LogLevel.Error);
                MemoryLogger.Logs.Last().Exception.Should().BeOfType<NullReferenceException>();
            }
            finally
            {
                semaphore.Release();
            }
        }

        private TrackingMiddleware CreateTrackingMiddleware(Action<PalmalyticsOptions> configure = null)
        {
            var services = new ServiceCollection();

            services.AddPalmalytics(x =>
            {
                x.AutomaticallyDownloadGeocodingData = false;
                x.UseDataStore(typeof(MemoryDataStore), null);
                configure?.Invoke(x);
            });

            services.AddTransient<TrackingMiddleware>();
            services.AddSingleton(typeof(ILogger<>), typeof(MemoryLogger<>));

            var provider = services.BuildServiceProvider();

            return provider.GetRequiredService<TrackingMiddleware>();
        }

        private RequestDelegate CreateNextRequestDelegate(
            int statusCode = 200,
            string contentType = "text/html",
            int contentLength = 100_000,
            Dictionary<string, StringValues> headers = null)
        {
            return context =>
            {
                headers ??= new Dictionary<string, StringValues>();

                var contextMock = (HttpContextMock)context;

                contextMock
                    .SetupResponseStatusCode(statusCode)
                    .SetupResponseContentType(contentType)
                    .SetupResponseContentLength(contentLength)
                    .SetupResponseHeaders(headers);

                return Task.Delay(3);
            };
        }
    }
}
