using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Palmalytics.Model;

namespace Palmalytics
{
    public interface IDataStore
    {
        void Initialize();
        Task AddRequestAsync(RequestData requestData);

        bool NeedsGeocodingDatabase();
        void ImportGeocodingData(IEnumerable<GeolocRange> data);
        string GetCountryCodeForIPAddress(IPAddress ipAddress);

        T GetSetting<T>(string name);
        Settings GetSettings();
        void SaveSetting<T>(string name, T value);
        void SaveSettings(Settings settings);

        List<Request> GetLastRequests(int count = 100);

        TopData GetTopData(DateTime? dateFrom, DateTime? dateTo, Filters filters);
        ChartData GetChart(DateTime? dateFrom, DateTime? dateTo, Interval interval, string property, Filters filters);
        TableData GetReferrers(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page);
        TableData GetUtmParameters(DateTime? dateFrom, DateTime? dateTo, string parameter, Filters filters, int page);
        TableData GetCountries(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page);
        TableData GetTopPages(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page);
        TableData GetEntryPages(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page);
        TableData GetExitPages(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page);
        TableData GetBrowsers(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page);
        TableData GetOperatingSystems(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page);
    }
}
