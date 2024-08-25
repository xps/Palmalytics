using System.Collections.Generic;

namespace Palmalytics.Services
{
    public interface IReferrerParser
    {
        void Initialize();
        void Initialize(IEnumerable<string> publicSuffixDomains);

        string GetReferrerName(string referrerUrl);
    }
}
