using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Palmalytics.Extensions;

namespace Palmalytics.Services
{
    public class ReferrerParser(ILogger<ReferrerParser> logger) : IReferrerParser
    {
        private HashSet<string> _domains;
        private static object initializationLock = new();

        public void Initialize()
        {
            lock (initializationLock)
            {
                if (_domains == null)
                {
                    logger.LogInformation("Initializing ReferrerParser");

                    _domains = ReadDomainsFromDataFile();
                    logger.LogDebug("Found {count:N0} domains", _domains.Count);
                }
            }
        }

        public void Initialize(IEnumerable<string> publicSuffixDomains)
        {
            logger.LogInformation("Initializing ReferrerParser");

            _domains = new HashSet<string>(publicSuffixDomains.Select(x => x.ToLower().Trim()));
        }

        public string GetReferrerName(string referrerUrl)
        {
            var domain = GetRegistrableDomain(referrerUrl);
            var name = GetNameForDomain(domain);
            return name;
        }

        protected string GetNameForDomain(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return null;

            if (Regex.IsMatch(domain, @"^google\.[a-z]{2,3}(\.[a-z]{2,3})?$"))
                return "Google";

            if (Regex.IsMatch(domain, @"^yahoo\.[a-z]{2,3}(\.[a-z]{2,3})?$"))
                return "Yahoo";

            if (Regex.IsMatch(domain, @"^yandex\.[a-z]{2,3}(\.[a-z]{2,3})?$"))
                return "Yandex";

            return domain;
        }

        protected string GetRegistrableDomain(string referrerUrl)
        {
            if (string.IsNullOrWhiteSpace(referrerUrl))
                return null;

            if (_domains == null)
                Initialize();

            if (!referrerUrl.Contains("://"))
                referrerUrl = "https://" + referrerUrl;

            try
            {
                var uri = new Uri(referrerUrl);
                var host = uri.Host.ToLower().TrimStart("www.");

                if (IPAddress.TryParse(host, out _))
                    return host;

                var parts = host.Split('.');

                for (var i = parts.Length - 1; i >= 0; i--)
                {
                    var domain = string.Join('.', parts[i..]);
                    if (!_domains.Contains(domain))
                        return domain;
                }

                return host;
            }
            catch
            {
                return null;
            }
        }

        private static HashSet<string> ReadDomainsFromDataFile()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream("Palmalytics.Data.public_suffix_list.dat.gz") ??
                throw new FileNotFoundException("Could not find resource public_suffix_list.dat.gz");

            using var decompressionStream = new GZipStream(stream, CompressionMode.Decompress);
            using var textReader = new StreamReader(decompressionStream);

            var domains = new HashSet<string>();

            // Read all lines from the stream
            string line;
            while ((line = textReader.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("//"))
                    domains.Add(line.ToLower().Trim());
            }

            return domains;
        }
    }
}
