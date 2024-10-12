using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Palmalytics.Dashboard;
using Palmalytics.Tests.TestHelpers;
using static Palmalytics.Dashboard.RequestHandlerBase;

namespace Palmalytics.Tests.Dashboard
{
    public class DashboardRequestHandlerTests(DashboardRequestHandler requestHandler)
    {
        private readonly DashboardRequestHandler requestHandler = requestHandler;

        [Fact]
        public void Test_DashboardRequestHandler_Returns_Index()
        {
            // Arrange
            var request = TestRequestsHelper.CreateRequest("/");

            // Act
            var result = requestHandler.GetResultForRequest(request);

            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be(200);
            result.ContentType.Should().Be("text/html");
            result.Should().BeOfType<StringResult>();
            var body = ((StringResult)result).Body;
            var parser = new HtmlParser();
            var document = parser.ParseDocument(body);
            var title = document.QuerySelector("title").Text();
            title.Should().Contain("Palmalytics");
        }

        [Fact]
        public void Test_DashboardRequestHandler_Returns_Static_Files()
        {
            // Arrange
            var request = TestRequestsHelper.CreateRequest("/images/palmalytics.svg");

            // Act
            var result = requestHandler.GetResultForRequest(request);

            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be(200);
            result.ContentType.Should().Be("image/svg+xml");
            result.Should().BeOfType<BytesResult>();
            var body = ((BytesResult)result).Body;
            var svg = Encoding.UTF8.GetString(body);
            svg.Should().Contain("<svg");
        }
    }
}
