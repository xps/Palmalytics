using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Palmalytics.SqlServer.Tests
{
    public static class DataStoreHelpers
    {
        // Creates a data store with an empty temporary database schema
        public static SqlServerDataStore CreateTemporaryDataStore(Action<SqlServerOptions>? configure = null)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", optional: false)
                .Build();

            // Check config
            var options = configuration.Get<TestOptions>();
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
                throw new ConfigurationErrorsException("ConnectionString must be set in appsettings.json");
            if (string.IsNullOrWhiteSpace(options.TemporarySchema))
                throw new ConfigurationErrorsException("TemporarySchema must be set in appsettings.json");

            var services = new ServiceCollection();

            services.AddLogging();
            services.Configure<TestOptions>(configuration);

            var sqlServerOptions = new SqlServerOptions
            {
                ConnectionString = options.ConnectionString,
                Schema = options.TemporarySchema,
                AlwaysDropAndCreateDatabase = true
            };

            configure?.Invoke(sqlServerOptions);

            services.AddPalmalytics(options =>
            {
                options.UseSqlServer(sqlServerOptions);
            });

            var serviceProvider = services.BuildServiceProvider();

            var dataStore = serviceProvider.GetRequiredService<IDataStore>();
            dataStore.Initialize();

            return (SqlServerDataStore)serviceProvider.GetRequiredService<IDataStore>();
        }

        // Create a data store with a test data set that persists between tests
        public static SqlServerDataStore CreatePersistentDataStore(Action<SqlServerOptions>? configure = null)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", optional: false)
                .Build();

            // Check config
            var options = configuration.Get<TestOptions>();
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
                throw new ConfigurationErrorsException("ConnectionString must be set in appsettings.json");
            if (string.IsNullOrWhiteSpace(options.PersistentSchema))
                throw new ConfigurationErrorsException("PersistentSchema must be set in appsettings.json");

            var services = new ServiceCollection();

            services.AddLogging();
            services.Configure<TestOptions>(configuration);

            var sqlServerOptions = new SqlServerOptions
            {
                ConnectionString = options.ConnectionString,
                Schema = options.PersistentSchema,
                AlwaysDropAndCreateDatabase = false
            };

            configure?.Invoke(sqlServerOptions);

            services.AddPalmalytics(options =>
            {
                options.UseSqlServer(sqlServerOptions);
            });

            var serviceProvider = services.BuildServiceProvider();

            var dataStore = serviceProvider.GetRequiredService<IDataStore>();
            dataStore.Initialize();

            return (SqlServerDataStore)serviceProvider.GetRequiredService<IDataStore>();
        }
    }
}
