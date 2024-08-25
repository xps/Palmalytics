using System;
using System.Security.Cryptography;
using System.Text;

namespace Palmalytics.Model
{
    // This is the request data collected from the web context
    public class RequestData
    {
        public long Id { get; set; }
        public long SessionId { get; set; }
        public DateTime DateUtc { get; set; }
        public string IPAddress { get; set; }
        public string Path { get; set; }
        public string QueryString { get; set; }
        public string UserAgent { get; set; }
        public string Language { get; set; }
        public string Country { get; set; }
        public string BrowserName { get; set; }
        public string BrowserVersion { get; set; }
        public string OSName { get; set; }
        public string OSVersion { get; set; }
        public bool? IsBot { get; set; }
        public string Referrer { get; set; }
        public string ReferrerName { get; set; }
        public string UtmSource { get; set; }
        public string UtmMedium { get; set; }
        public string UtmCampaign { get; set; }
        public string UtmTerm { get; set; }
        public string UtmContent { get; set; }
        public string UserName { get; set; }
        public string CustomData { get; set; }
        public int? ResponseCode { get; set; }
        public int ResponseTime { get; set; } // in ms
        public string ContentType { get; set; }

        public int GetSessionHashCode()
        {
            var str = string.Concat(IPAddress, UserAgent, Language, BrowserName, BrowserVersion, OSName, OSVersion);
            var hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(str));
            var integer = BitConverter.ToInt32(hash, 0);
            return integer;
        }

        public bool MatchesSession(Session session)
        {
            return IPAddress == session.IPAddress &&
                   UserAgent == session.UserAgent &&
                   Language == session.Language;
        }
    }
}
