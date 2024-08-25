using System.Collections.Generic;
using System.Threading.Tasks;
using Palmalytics.Model;

namespace Palmalytics.Services
{
    public interface IGeocodingDataProvider
    {
        Task<List<GeolocRange>> DownloadGeocodingDataAsync(string version);
    }
}
