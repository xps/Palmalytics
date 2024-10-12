namespace Palmalytics.Model
{
    public class Filters
    {
        public string Browser { get; set; }
        public string BrowserVersion { get; set; }

        public string OS { get; set; }
        public string OSVersion { get; set; }

        public string Referrer { get; set; }
        public string ReferrerUrl { get; set; }

        public string UtmSource { get; set; }
        public string UtmMedium { get; set; }
        public string UtmCampaign { get; set; }
        public string UtmContent { get; set; }
        public string UtmTerm { get; set; }

        public string Country { get; set; }

        public string Path { get; set; }
        public string EntryPath { get; set; }
        public string ExitPath { get; set; }
    }
}
