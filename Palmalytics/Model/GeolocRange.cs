using System.Net;

namespace Palmalytics.Model
{
    public class GeolocRange(IPAddress start, IPAddress end, string country)
    {
        public IPAddress Start { get; set; } = start;
        public IPAddress End { get; set; } = end;
        public string Country { get; set; } = country;
    }
}
