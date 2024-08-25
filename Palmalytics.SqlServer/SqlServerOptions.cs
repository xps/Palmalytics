namespace Palmalytics.SqlServer
{
    public class SqlServerOptions
    {
        public required string ConnectionString { get; set; }
        public string Schema { get; set; } = "dbo";

        public bool AlwaysDropAndCreateDatabase { get; set; } = false;
        public int IngestionCommandTimeout { get; set; } = 10; // Seconds

        public string RequestsTable { get; set; } = "Requests";
        public string SessionsTable { get; set; } = "Sessions";
        public string GeocodingTable { get; set; } = "Geocoding";
        public string SettingsTable { get; set; } = "Settings";

        public int MinutesBeforeNewSession { get; set; } = 30;
    }
}