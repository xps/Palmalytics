using System.Net;
using Microsoft.Extensions.Options;
using Palmalytics.Model;

namespace Palmalytics.Tests.TestHelpers
{
    public class MemoryDataStore : IDataStore
    {
        public List<Request> requests { get; private set; }
        public Dictionary<string, object> settings { get; private set; }

        public MemoryDataStore(IOptions<object> options)
        {
        }

        public void Initialize()
        {
            requests = new();
            settings = new();
        }

        public Task AddRequestAsync(RequestData requestData)
        {
            requests.Add(new Request
            {
                Id = requestData.Id,
                SessionId = requestData.SessionId,
                DateUtc = requestData.DateUtc,
                Path = requestData.Path,
                QueryString = requestData.QueryString,
                IsBot = requestData.IsBot,
                Referrer = requestData.Referrer,
                UtmSource = requestData.UtmSource,
                UtmMedium = requestData.UtmMedium,
                UtmCampaign = requestData.UtmCampaign,
                UtmTerm = requestData.UtmTerm,
                UtmContent = requestData.UtmContent,
                UserName = requestData.UserName,
                CustomData = requestData.CustomData,
                ResponseCode = requestData.ResponseCode,
                ResponseTime = requestData.ResponseTime,
                ContentType = requestData.ContentType
            });
            return Task.CompletedTask;
        }

        public List<Request> GetLastRequests(int count = 100)
        {
            return requests
                .OrderByDescending(r => r.DateUtc)
                .Take(count)
                .ToList();
        }

        public TopData GetTopData(DateTime? dateFrom, DateTime? dateTo, Filters filters)
        {
            throw new NotImplementedException();
        }

        public ChartData GetChart(DateTime? dateFrom, DateTime? dateTo, Interval interval, string property, Filters filters)
        {
            throw new NotImplementedException();
        }

        public TableData GetBrowsers(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page)
        {
            throw new NotImplementedException();
        }

        public TableData GetOperatingSystems(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page)
        {
            throw new NotImplementedException();
        }

        public TableData GetReferrers(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page)
        {
            throw new NotImplementedException();
        }

        public TableData GetUtmParameters(DateTime? dateFrom, DateTime? dateTo, string parameter, Filters filters, int page)
        {
            throw new NotImplementedException();
        }

        public TableData GetCountries(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page)
        {
            throw new NotImplementedException();
        }

        public TableData GetTopPages(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page)
        {
            throw new NotImplementedException();
        }

        public TableData GetEntryPages(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page)
        {
            throw new NotImplementedException();
        }

        public TableData GetExitPages(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page)
        {
            throw new NotImplementedException();
        }

        public string GetCountryCodeForIPAddress(IPAddress ipAddress)
        {
            return null; // TODO: read from sample file
        }

        public bool NeedsGeocodingDatabase()
        {
            throw new NotImplementedException();
        }

        public void ImportGeocodingData(IEnumerable<GeolocRange> data)
        {
            throw new NotImplementedException();
        }

        #region Settings

        public T GetSetting<T>(string name)
        {
            return (T)settings[name];
        }

        public Settings GetSettings()
        {
            return new Settings
            {
                GeocodingDataVersion = settings.GetValueOrDefault("GeocodingDataVersion") as string,
                SchemaVersion = (int)settings.GetValueOrDefault("SchemaVersion", 1)
            };
        }

        public void SaveSetting<T>(string name, T value)
        {
            settings[name] = value;
        }

        public void SaveSettings(Settings settings)
        {
            this.settings["GeocodingDataVersion"] = settings.GeocodingDataVersion;
            this.settings["SchemaVersion"] = settings.SchemaVersion;
        }

        #endregion
    }
}
