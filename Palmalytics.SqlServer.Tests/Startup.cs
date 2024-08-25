using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Palmalytics.SqlServer.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //var configuration = new ConfigurationBuilder()
            //    .AddJsonFile($"appsettings.json", optional: false)
            //    .Build();

            //services.AddLogging();
            //services.Configure<TestOptions>(configuration);

            //// Check config
            //var options = configuration.Get<TestOptions>();
            //if (string.IsNullOrWhiteSpace(options.ConnectionString))
            //    throw new ConfigurationErrorsException("ConnectionString must be set in appsettings.json");
            //if (string.IsNullOrWhiteSpace(options.Schema))
            //    throw new ConfigurationErrorsException("Schema must be set in appsettings.json");
        }
    }
}
