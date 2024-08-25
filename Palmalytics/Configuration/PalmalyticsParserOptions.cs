namespace Palmalytics
{
    public class PalmalyticsParserOptions
    {
        public bool CollectQueryString { get; set; } = true;
        public bool CollectIPAddress { get; set; } = true;
        public bool CollectUserAgent { get; set; } = true;
        public bool CollectUserName { get; set; } = false;
        public bool CollectLanguage { get; set; } = true;
        public bool CollectCountry { get; set; } = true;
        public bool CollectReferrer { get; set; } = true;
        public bool CollectUtmParameters { get; set; } = true;
    }
}
