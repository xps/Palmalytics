using System;

namespace Palmalytics.Model
{
    public class Session
    {
        public long Id { get; set; }
        public int HashCode { get; set; }
        public DateTime DateStartedUtc { get; set; }
        public DateTime DateEndedUtc { get; set; }

        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public string Language { get; set; }
        public string Country { get; set; }
        public string BrowserName { get; set; }
        public string BrowserVersion { get; set; }
        public string OSName { get; set; }
        public string OSVersion { get; set; }

        public string EntryPath { get; set; }
        public string ExitPath { get; set; }
        public bool IsBounce { get; set; }

        public string Referrer { get; set; }
        public string ReferrerName { get; set; }
        public string UtmSource { get; set; }
        public string UtmMedium { get; set; }
        public string UtmCampaign { get; set; }
        public string UtmTerm { get; set; }
        public string UtmContent { get; set; }
        public string UserName { get; set; }
        public string CustomData { get; set; }

        public int Duration { get; set; }           // in seconds
        public int RequestCount { get; set; }

        public Session()
        {
        }

        public Session(RequestData request)
        {
            HashCode = request.GetSessionHashCode();
            DateStartedUtc = request.DateUtc;
            DateEndedUtc = request.DateUtc;

            IPAddress = request.IPAddress;
            UserAgent = request.UserAgent;
            Language = request.Language;
            Country = request.Country;
            BrowserName = request.BrowserName;
            BrowserVersion = request.BrowserVersion;
            OSName = request.OSName;
            OSVersion = request.OSVersion;

            EntryPath = request.Path;
            ExitPath = request.Path;
            IsBounce = true;

            Referrer = request.Referrer;
            ReferrerName = request.ReferrerName;
            UtmSource = request.UtmSource;
            UtmMedium = request.UtmMedium;
            UtmCampaign = request.UtmCampaign;
            UtmTerm = request.UtmTerm;
            UtmContent = request.UtmContent;
            UserName = request.UserName;
            CustomData = request.CustomData;

            RequestCount = 1;
            Duration = 0;
        }
    }
}
