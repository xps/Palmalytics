using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Palmalytics.Model;

namespace Palmalytics.Services
{
    public class GeocodingDataProvider(ILogger<GeocodingDataProvider> logger) : IGeocodingDataProvider
    {
        private readonly ILogger logger = logger;

        public async Task<List<GeolocRange>> DownloadGeocodingDataAsync(string version)
        {
            var data = new List<GeolocRange>();
            var url = $"https://download.db-ip.com/free/dbip-country-lite-{version}.csv.gz";

            logger.LogInformation("Downloading: {url}", url);

            using var httpClient = new HttpClient();
            using var webStream = await httpClient.GetStreamAsync(url);
            using var decompressionStream = new GZipStream(webStream, CompressionMode.Decompress);
            using var textReader = new StreamReader(decompressionStream);

            while (!textReader.EndOfStream)
            {
                var line = textReader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var parts = line.Split(',');
                    if (parts.Length == 3)
                    {
                        var start = IPAddress.Parse(parts[0]);
                        var end = IPAddress.Parse(parts[1]);
                        var country = parts[2];

                        data.Add(new GeolocRange(start, end, country.ToUpper()));
                    }
                }
            }

            logger.LogDebug("Found {count:N0} geocoding data entries", data.Count);

            if (!data.Any())
                throw new Exception("No geocoding data found"); // TODO: specific type of exception

            return data;
        }
    }
}
