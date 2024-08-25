using System;

namespace Palmalytics.Exceptions
{
    public class PalmalyticsTrackingException : Exception
    {
        public PalmalyticsTrackingException(string message) : base(message)
        {
        }

        public PalmalyticsTrackingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
