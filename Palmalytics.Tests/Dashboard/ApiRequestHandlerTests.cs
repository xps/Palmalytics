using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Palmalytics.Dashboard;
using Palmalytics.Model;
using Palmalytics.Tests.TestHelpers;
using static Palmalytics.Dashboard.RequestHandlerBase;

namespace Palmalytics.Tests.Dashboard
{
    public class ApiRequestHandlerTests
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ApiRequestHandler handler;

        public ApiRequestHandlerTests(IServiceProvider serviceProvider, IApiRequestHandler handler)
        {
            this.serviceProvider = serviceProvider;
            this.handler = (ApiRequestHandler)handler;
        }

        [Fact(Skip = "MemoryDataStore does not implement this method")]
        public void Test_ApiRequestHandler_GetChart()
        {
            // TODO: remove magic strings
            var result = handler.GetChart("last-30-days", Interval.Days, "Sessions", filters: null);

            result.Should().NotBeNull();
            result.Code.Should().Be(200);
            result.ContentType.Should().Be("application/json");
        }

        [Fact(Skip = "MemoryDataStore does not implement this method")]
        public void Test_ApiRequestHandler_GetBrowsers()
        {
            // TODO: remove magic string
            var result = handler.GetBrowsers("last-30-days", filters: null);

            result.Should().NotBeNull();
            result.Code.Should().Be(200);
            result.ContentType.Should().Be("application/json");
        }

        [Fact]
        public void Test_ApiRequestHandler_GetPerformanceStats()
        {
            var result = handler.GetPerformanceStats();

            result.Should().NotBeNull();
            result.Code.Should().Be(200);
            result.ContentType.Should().Be("application/json");
        }

        #region Tests for mapping requests to methods

        [Theory]
        [InlineData("/test-no-param", "GetTestNoParam")]
        [InlineData("/test-string-param?value=Foo", "GetTestStringParam: Foo")]
        [InlineData("/test-date-param?value=2023-09-16", "GetTestDateParam: 2023-09-16 00:00:00")]
        [InlineData("/test-nullable-date-param?value=2023-09-16", "GetTestNullableDateParam: 2023-09-16 00:00:00")]
        [InlineData("/test-bool-param?value=true", "GetTestBoolParam: True")]
        [InlineData("/test-nullable-bool-param?value=true", "GetTestNullableBoolParam: True")]
        [InlineData("/test-int-param?value=123456789", "GetTestIntParam: 123456789")]
        [InlineData("/test-nullable-int-param?value=123456789", "GetTestNullableIntParam: 123456789")]
        [InlineData("/test-enum-param?value=weeks", "GetTestEnumParam: Weeks")]
        [InlineData("/test-nullable-enum-param?value=weeks", "GetTestNullableEnumParam: Weeks")]
        [InlineData("/test-object-param?value1=123", "GetTestObjectParam: 123|0")]
        [InlineData("/test-multiple-params?x=123&z=true&y=Foo", "GetTestMultipleParams: 123|Foo|True")]
        [InlineData("/test-optional-param", "GetTestOptionalParam: 10")]
        public void Test_ApiRequestHandler_Method_Mapping(string path, string body)
        {
            var handler = serviceProvider.GetRequiredService<TestRequestHandler>();
            var request = TestRequestsHelper.CreateRequest(path: path);
            var result = (StringResult)handler.GetResultForRequest(request);

            result.Body.Should().Be(body);
        }

        [Theory]
        [InlineData("/test-string-param", "GetTestStringParam: ")]
        [InlineData("/test-date-param", "GetTestDateParam: 0001-01-01 00:00:00")]
        [InlineData("/test-nullable-date-param", "GetTestNullableDateParam: ")]
        [InlineData("/test-bool-param", "GetTestBoolParam: False")]
        [InlineData("/test-nullable-bool-param", "GetTestNullableBoolParam: ")]
        [InlineData("/test-int-param", "GetTestIntParam: 0")]
        [InlineData("/test-nullable-int-param", "GetTestNullableIntParam: ")]
        [InlineData("/test-enum-param", "GetTestEnumParam: Days")]
        [InlineData("/test-nullable-enum-param", "GetTestNullableEnumParam: ")]
        [InlineData("/test-object-param", "GetTestObjectParam: 0|0")]
        [InlineData("/test-multiple-params", "GetTestMultipleParams: 0||False")]
        public void Test_ApiRequestHandler_Method_Mapping_Missing_Param_Gets_Default_Value(string path, string body)
        {
            var handler = serviceProvider.GetRequiredService<TestRequestHandler>();
            var request = TestRequestsHelper.CreateRequest(path: path);
            var result = (StringResult)handler.GetResultForRequest(request);

            result.Body.Should().Be(body);
        }

        [Fact]
        public void Test_ApiRequestHandler_Method_Mapping_Bad_Param()
        {
            var handler = serviceProvider.GetRequiredService<TestRequestHandler>();
            var request = TestRequestsHelper.CreateRequest(path: "/test-int-param?value=x");
            var result = (StringResult)handler.GetResultForRequest(request);

            result.Code.Should().Be(400);
            result.Body.Should().Be("The input string 'x' was not in a correct format.");
        }

        internal class TestRequestHandler : ApiRequestHandler
        {
            public TestRequestHandler(IDataStore dataStore, IOptions<PalmalyticsOptions> options, ILogger<ApiRequestHandler> logger) : base(dataStore, options, logger)
            {
            }

            public Result GetTestNoParam()
            {
                return Content("GetTestNoParam", "text/plain");
            }

            public Result GetTestStringParam(string value)
            {
                return Content($"GetTestStringParam: {value}", "text/plain");
            }

            public Result GetTestDateParam(DateTime value)
            {
                return Content($"GetTestDateParam: {value:yyyy-MM-dd HH:mm:ss}", "text/plain");
            }

            public Result GetTestNullableDateParam(DateTime? value)
            {
                return Content($"GetTestNullableDateParam: {value:yyyy-MM-dd HH:mm:ss}", "text/plain");
            }

            public Result GetTestBoolParam(bool value)
            {
                return Content($"GetTestBoolParam: {value}", "text/plain");
            }

            public Result GetTestNullableBoolParam(bool? value)
            {
                return Content($"GetTestNullableBoolParam: {value}", "text/plain");
            }

            public Result GetTestIntParam(int value)
            {
                return Content($"GetTestIntParam: {value}", "text/plain");
            }

            public Result GetTestNullableIntParam(int? value)
            {
                return Content($"GetTestNullableIntParam: {value}", "text/plain");
            }

            public Result GetTestEnumParam(Interval value)
            {
                return Content($"GetTestEnumParam: {value}", "text/plain");
            }

            public Result GetTestNullableEnumParam(Interval? value)
            {
                return Content($"GetTestNullableEnumParam: {value}", "text/plain");
            }

            public Result GetTestMultipleParams(int x, string y, bool z)
            {
                return Content($"GetTestMultipleParams: {x}|{y}|{z}", "text/plain");
            }

            public Result GetTestOptionalParam(int value = 10)
            {
                return Content($"GetTestOptionalParam: {value}", "text/plain");
            }

            public Result GetTestObjectParam(ObjectParam obj)
            {
                return Content($"GetTestObjectParam: {obj?.Value1}|{obj?.Value2}", "text/plain");
            }

            public class ObjectParam
            {
                public int Value1 { get; set; }
                public int Value2 { get; set; }
            }
        }

        #endregion
    }
}
