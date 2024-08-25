using System;
using Palmalytics.Exceptions;

namespace Palmalytics.SqlServer
{
    public static class PalmalyticsOptionsExtensions
    {
        public static void UseSqlServer(this PalmalyticsOptions options, SqlServerOptions sqlServerOptions)
        {
            if (sqlServerOptions == null)
                throw new ArgumentNullException(nameof(sqlServerOptions), "Options cannot be null");
            if (string.IsNullOrWhiteSpace(sqlServerOptions.ConnectionString))
                throw new PalmalyticsOptionsException("ConnectionString cannot be null or empty");

            options.UseDataStore(typeof(SqlServerDataStore), sqlServerOptions);
        }
    }
}
