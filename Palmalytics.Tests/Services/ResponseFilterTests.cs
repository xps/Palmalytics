using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Palmalytics.Services;
using static Palmalytics.Tests.TestHelpers.TestResponsesHelper;

namespace Palmalytics.Tests.Services
{
    public class ResponseFilterTests
    {
        private readonly IServiceProvider serviceProvider;

        public ResponseFilterTests(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [Fact]
        public void Test_RequestFilter_IncludeStatusCodes_Default()
        {
            var options = new PalmalyticsFilterOptions();

            var filter = CreateResponseFilter(options);

            filter.ShouldTrackResponse(CreateResponse(statusCode: 200)).Should().BeTrue();
            filter.ShouldTrackResponse(CreateResponse(statusCode: 404)).Should().BeFalse();
        }

        [Fact]
        public void Test_RequestFilter_IncludeStatusCodes_Custom()
        {
            var options = new PalmalyticsFilterOptions();
            options.IncludeStatusCodes.Add(201);
            options.IncludeStatusCodes.Add(404);

            var filter = CreateResponseFilter(options);

            filter.ShouldTrackResponse(CreateResponse(statusCode: 200)).Should().BeTrue();
            filter.ShouldTrackResponse(CreateResponse(statusCode: 201)).Should().BeTrue();
            filter.ShouldTrackResponse(CreateResponse(statusCode: 404)).Should().BeTrue();
            filter.ShouldTrackResponse(CreateResponse(statusCode: 500)).Should().BeFalse();
        }

        [Theory]
        [InlineData("text/html", true)]
        [InlineData("text/plain", false)]
        [InlineData("application/json", false)]
        [InlineData(null, false)]
        public void Test_RequestFilter_IncludeContentTypes_Default(string contentType, bool shouldTrack)
        {
            var options = new PalmalyticsFilterOptions();
            var filter = CreateResponseFilter(options);

            var request = CreateResponse(contentType: contentType);
            var requestWithCharset = CreateResponse(contentType: contentType + "; charset=utf-8");

            filter.ShouldTrackResponse(request).Should().Be(shouldTrack);
            filter.ShouldTrackResponse(requestWithCharset).Should().Be(shouldTrack);
        }

        [Theory]
        [InlineData("text/html", true)]
        [InlineData("text/plain", true)]
        [InlineData("application/json", false)]
        [InlineData(null, false)]
        public void Test_RequestFilter_IncludeContentTypes_Custom(string contentType, bool shouldTrack)
        {
            var options = new PalmalyticsFilterOptions();
            options.IncludeContentTypes.Add("text/plain");

            var filter = CreateResponseFilter(options);

            var request = CreateResponse(contentType: contentType);
            var requestWithCharset = CreateResponse(contentType: contentType + "; charset=utf-8");

            filter.ShouldTrackResponse(request).Should().Be(shouldTrack);
            filter.ShouldTrackResponse(requestWithCharset).Should().Be(shouldTrack);
        }

        [Fact]
        public void Test_RequestFilter_IncludeContentTypes_IgnoresCase()
        {
            var options = new PalmalyticsFilterOptions();
            options.IncludeContentTypes.Add("text/plain");

            var filter = CreateResponseFilter(options);

            var textHtml = CreateResponse(contentType: "Text/Html");
            var textPlain = CreateResponse(contentType: "Text/Plain");
            var applicationJson = CreateResponse(contentType: "Application/Json");

            filter.ShouldTrackResponse(textHtml).Should().BeTrue();
            filter.ShouldTrackResponse(textPlain).Should().BeTrue();
            filter.ShouldTrackResponse(applicationJson).Should().BeFalse();
        }

        private ResponseFilter CreateResponseFilter(PalmalyticsFilterOptions options)
        {
            return ActivatorUtilities.CreateInstance<ResponseFilter>(serviceProvider, Options.Create(options));
        }
    }
}
