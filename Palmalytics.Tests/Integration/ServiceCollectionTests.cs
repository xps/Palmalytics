using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Palmalytics.Exceptions;
using Palmalytics.Services;
using Palmalytics.Tests.TestHelpers;

namespace Palmalytics.Tests.Integration
{
    public class ServiceCollectionTests
    {
        [Fact]
        public void Test_AddPalmalytics_Registers_All_Services()
        {
            var services = new ServiceCollection();

            services.AddLogging();

            services.AddPalmalytics(options =>
            {
                options.UseDataStore<MemoryDataStore>();
            });

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<IOptions<PalmalyticsOptions>>();
            serviceProvider.GetRequiredService<IOptions<PalmalyticsFilterOptions>>();
            serviceProvider.GetRequiredService<IOptions<PalmalyticsParserOptions>>();

            serviceProvider.GetRequiredService<IRequestParser>().Should().BeOfType<RequestParser>();
            serviceProvider.GetRequiredService<IRequestFilter>().Should().BeOfType<RequestFilter>();
            serviceProvider.GetRequiredService<IResponseFilter>().Should().BeOfType<ResponseFilter>();
            serviceProvider.GetRequiredService<IUserAgentParser>().Should().BeOfType<FastUserAgentParser>();

            serviceProvider.GetRequiredService<IDataStore>().Should().BeOfType<MemoryDataStore>();
        }

        [Fact]
        public void Test_AddPalmalytics_Passes_All_Options()
        {
            var defaultConfig = new PalmalyticsOptions();

            var services = new ServiceCollection();

            services.AddLogging();

            services.AddPalmalytics(options =>
            {
                options.MaxErrorsBeforeFail = 123;
                options.FilterOptions.IncludeStatusCodes.Add(456);
                options.ParserOptions.CollectIPAddress = false;
                options.UseDataStore<MemoryDataStore>();
            });

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<IOptions<PalmalyticsOptions>>().Value.MaxErrorsBeforeFail
                .Should().Be(123)
                .And.NotBe(defaultConfig.MaxErrorsBeforeFail);

            serviceProvider.GetRequiredService<IOptions<PalmalyticsFilterOptions>>().Value.IncludeStatusCodes
                .Should().Contain(456)
                .And.NotBeEquivalentTo(defaultConfig.FilterOptions.IncludeStatusCodes);

            serviceProvider.GetRequiredService<IOptions<PalmalyticsParserOptions>>().Value.CollectIPAddress
                .Should().BeFalse()
                .And.NotBe(defaultConfig.ParserOptions.CollectIPAddress);

            var requestParser = (RequestParser)serviceProvider.GetRequiredService<IRequestParser>();
            var requestFilter = (RequestFilter)serviceProvider.GetRequiredService<IRequestFilter>();
            var responseFilter = (ResponseFilter)serviceProvider.GetRequiredService<IResponseFilter>();

            requestParser.Options.CollectIPAddress.Should().BeFalse()
                .And.NotBe(defaultConfig.ParserOptions.CollectIPAddress);

            requestFilter.Options.IncludeStatusCodes.Should().Contain(456)
                .And.NotBeEquivalentTo(defaultConfig.FilterOptions.IncludeStatusCodes);

            responseFilter.Options.IncludeStatusCodes.Should().Contain(456)
                .And.NotBeEquivalentTo(defaultConfig.FilterOptions.IncludeStatusCodes);

            serviceProvider.GetRequiredService<IDataStore>().Should().BeOfType<MemoryDataStore>();
        }

        [Fact]
        public void Test_AddPalmalytics_Throws_If_No_DataStore()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddPalmalytics(options => { });

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.Invoking(x => x.GetRequiredService<IDataStore>())
                .Should().Throw<PalmalyticsOptionsException>()
                .Which.Message.Should().Be("No data store configured");
        }
    }
}
