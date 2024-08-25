using System;

namespace Palmalytics.Model
{
    // This is a request as stored in the datastore
    public class Request
    {
        public long Id { get; set; }
        public long SessionId { get; set; }
        public DateTime DateUtc { get; set; }
        public string Path { get; set; }
        public string QueryString { get; set; }
        public bool? IsBot { get; set; }        // TODO: move to session
        public string Referrer { get; set; }
        public string UtmSource { get; set; }
        public string UtmMedium { get; set; }
        public string UtmCampaign { get; set; }
        public string UtmTerm { get; set; }
        public string UtmContent { get; set; }
        public string UserName { get; set; }
        public string CustomData { get; set; }
        public int? ResponseCode { get; set; }
        public int ResponseTime { get; set; }   // in ms
        public string ContentType { get; set; }
    }
}
