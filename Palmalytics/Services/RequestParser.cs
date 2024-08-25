using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Palmalytics.Extensions;
using Palmalytics.Model;

namespace Palmalytics.Services
{
    public class RequestParser : IRequestParser
    {
        private readonly IUserAgentParser userAgentParser;
        private readonly IReferrerParser referrerParser;
        private readonly PalmalyticsOptions topLevelOptions;
        private readonly PalmalyticsParserOptions options;
        private readonly ILogger logger;

        public PalmalyticsParserOptions Options => options;

        private static HashSet<string> errors = [];

        public RequestParser(
            IUserAgentParser userAgentParser,
            IReferrerParser referrerParser,
            IOptions<PalmalyticsOptions> topLevelOptions,
            IOptions<PalmalyticsParserOptions> options,
            ILogger<RequestParser> logger)
        {
            this.userAgentParser = userAgentParser;
            this.referrerParser = referrerParser;
            this.topLevelOptions = topLevelOptions.Value;
            this.options = options.Value;
            this.logger = logger;
        }

        public virtual RequestData Parse(HttpRequest request)
        {
            var requestData = new RequestData
            {
                DateUtc = DateTime.UtcNow,
                Path = request.Path
            };

            if (options.CollectQueryString)
                requestData.QueryString = request.QueryString.ToString().TrimStart('?').NullIfEmpty();

            if (options.CollectReferrer)
            {
                requestData.Referrer = request.Headers["Referer"];
                requestData.ReferrerName = referrerParser.GetReferrerName(requestData.Referrer)?.NullIfEmpty();
            }

            if (options.CollectIPAddress)
                requestData.IPAddress = topLevelOptions.GetClientIPAddress(request).ToString();

            if (options.CollectUserAgent)
            {
                requestData.UserAgent = request.Headers["User-Agent"];

                try
                {
                    var device = userAgentParser.GetDevice(request);
                    if (device != null)
                    {
                        requestData.BrowserName = device.BrowserName;
                        requestData.BrowserVersion = device.BrowserVersion;
                        requestData.OSName = device.OSName;
                        requestData.OSVersion = device.OSVersion;
                        requestData.IsBot = device.IsBot;
                    }
                    else if (string.IsNullOrWhiteSpace(requestData.UserAgent))
                    {
                        requestData.IsBot = true;
                    }
                }
                catch (Exception x)
                {
                    logger.LogError(x, "Failed to parse request's user agent. Request:\n{request}", request.GetDebugString());
                }
            }

            if (options.CollectLanguage)
                requestData.Language = ParseLanguage(request.Headers["Accept-Language"]);

            if (options.CollectUserName)
                requestData.UserName = request.HttpContext.User.Identity.Name;

            if (options.CollectUtmParameters)
            {
                requestData.UtmSource = request.Query["utm_source"];
                requestData.UtmMedium = request.Query["utm_medium"];
                requestData.UtmCampaign = request.Query["utm_campaign"];
                requestData.UtmTerm = request.Query["utm_term"];
                requestData.UtmContent = request.Query["utm_content"];
            }

            return requestData;
        }

        // See: https://source.dot.net/#Microsoft.AspNetCore.Localization/AcceptLanguageHeaderRequestCultureProvider.cs
        public virtual string ParseLanguage(string acceptLanguageHeader)
        {
            if (!string.IsNullOrWhiteSpace(acceptLanguageHeader))
            {
                var str = acceptLanguageHeader.ToString().Trim();
                if (str.Length >= 2)
                {
                    try
                    {
                        // Since languages are typically ordered by priority, we'll just take the first one
                        var language = str.Capture("^([a-z]{2}(-[a-z]{2})?)", RegexOptions.IgnoreCase);

                        // The convetion is that the first two characters are lower case and the rest are upper case
                        if (language.Length > 2)
                            language = language[..2].ToLower() + language[2..].ToUpper();
                        else
                            language = language.ToLower();

                        return language;
                    }
                    catch (Exception x)
                    {
                        if (!errors.Contains(acceptLanguageHeader))
                        {
                            logger.LogWarning(x, "Could not parse Accept-Language header. Value = {header}", acceptLanguageHeader);
                            errors.Add(acceptLanguageHeader);
                        }
                    }
                }
            }

            return null;
        }
    }
}
