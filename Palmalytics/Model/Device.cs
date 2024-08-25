namespace Palmalytics.Model
{
    public class Device
    {
        public string OSName { get; set; }
        public string OSVersion { get; set; }
        public string BrowserName { get; set; }
        public string BrowserVersion { get; set; }
        public bool? IsMobile { get; set; }
        public bool IsBot { get; set; }
    }
}
