using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Palmalytics.Extensions;
using Palmalytics.Model;

namespace Palmalytics.Services
{
    public class FastUserAgentParser(ILogger<FastUserAgentParser> logger) : IUserAgentParser
    {
        private readonly ILogger logger = logger;

        private static readonly string[] BotKeywords = ["google", "bot", "spider", "crawler", "spider", "http", "https", "feed", "archive", "index", "search", "monitor", "watcher", "check", "validator", "validator", "validator", "preview", "verification", "agent", "mailto", ".com"];

        // Regex validating an entry in the Sec-CH-UA header, e.g. `"Google Chrome";v="116"`
        private static readonly Regex clientHintBrandRegex = new(
            """
            "([^"]*)"\s*;\s*v\s*=\s*"([^"]*)"
            """,
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // Maps known values for the Sec-CH-UA header to the values we want to display
        private readonly Dictionary<string, string> ClientHintsBrowsers = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Google Chrome", "Chrome" },
            { "Microsoft Edge", "Edge" },
            { "Safari", "Safari" },
            { "Firefox", "Firefox" },
            { "Brave", "Brave" },
            { "Samsung Internet", "Samsung Internet" },
            { "Opera", "Opera" },
            { "Opera GX", "Opera" },
            { "OperaMobile", "Opera" },
            { "DuckDuckGo", "DuckDuckGo" },
            { "Vivaldi", "Vivaldi" },
            { "Yandex", "Yandex" },
            { "XiaoMiBrowser", "Xiaomi Browser" }
        };

        // Use a ConcurrentDictionary since there is no ConcurrentHashSet in the BCL :'(
        private static readonly ConcurrentDictionary<string, byte> loggedClientHintsErrors = new();
        private const int maxLoggedClientHintsErrors = 50;

        // Brands in the Sec-CH-UA header that indicate a bot
        private static readonly string[] ClientHintsBotBrands = ["HeadlessChrome"];

        // Maps known values for the Sec-CH-UA-Platform header to the values we want to display
        private static readonly Dictionary<string, string> ClientHintsPlatforms = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Windows", "Windows" },
            { "macOS", "Mac" },
            { "Linux", "Linux" },
            { "Android", "Android" },
            { "iOS", "iOS" },
            { "Chrome OS", "Chrome OS" },
            { "Chromium OS", "Chrome OS" },
            { "Unknown", null }
        };

        public virtual Device GetDevice(HttpRequest request)
        {
            // Start with the user agent
            var userAgent = request.Headers["User-Agent"].ToString();
            var device = !string.IsNullOrWhiteSpace(userAgent) ? ParseUserAgent(userAgent) : null;

            if (request.Headers.ContainsKey("Sec-CH-UA"))
            {
                // Then read client hints
                var hints = ParseClientHints(request.Headers, out var isGenericBrowser);

                // If nothing from user agent keep client hints only
                if (device == null)
                    return hints;

                if (hints.IsBot)
                {
                    device.IsBot = true;
                    return device;
                }

                // We got both, combine them
                CombineDeviceInfo(device, hints, isGenericBrowser);
            }

            return device;
        }

        private static void CombineDeviceInfo(Device userAgentDevice, Device clientHintsDevice, bool isGenericBrowser)
        {
            // A generic browser from client hints (plain Chromium) should not override a more specific one from the user agent
            var keepUserAgentBrowser = isGenericBrowser && !string.IsNullOrWhiteSpace(userAgentDevice.BrowserName);

            if (!string.IsNullOrWhiteSpace(clientHintsDevice.BrowserName) && !keepUserAgentBrowser)
            {
                userAgentDevice.BrowserName = clientHintsDevice.BrowserName;
                userAgentDevice.BrowserVersion = clientHintsDevice.BrowserVersion;
            }

            if (!string.IsNullOrWhiteSpace(clientHintsDevice.OSName))
            {
                // If we're going to keep the client hints OS (below) and it's different from the user agent OS
                // we can't keep the user agent OS version
                if (!string.Equals(userAgentDevice.OSName, clientHintsDevice.OSName, StringComparison.OrdinalIgnoreCase))
                    userAgentDevice.OSVersion = null;

                userAgentDevice.OSName = clientHintsDevice.OSName;
            }

            if (!string.IsNullOrWhiteSpace(clientHintsDevice.OSVersion))
                userAgentDevice.OSVersion = clientHintsDevice.OSVersion;

            if (clientHintsDevice.IsMobile != null)
                userAgentDevice.IsMobile = clientHintsDevice.IsMobile;
        }

        public virtual bool DetectBot(string userAgent)
        {
            // If there's no user agent or it is suspiciously short, assume it's a bot
            if (string.IsNullOrWhiteSpace(userAgent) || userAgent.Length < 50)
                return true;

            // Check for bots keywords
            if (BotKeywords.Any(x => userAgent.Contains(x, StringComparison.OrdinalIgnoreCase)))
                return true;

            return false;
        }

        public virtual Device ParseClientHints(IHeaderDictionary headers)
        {
            return ParseClientHints(headers, out _);
        }

        private Device ParseClientHints(IHeaderDictionary headers, out bool isGenericBrowser)
        {
            var device = new Device();
            isGenericBrowser = false;

            if (headers.ContainsKey("Sec-CH-UA"))
            {
                var brands = ParseClientHintUserAgent(headers["Sec-CH-UA"].ToString());
                if (brands.Keys.Any(x => ClientHintsBotBrands.Contains(x, StringComparer.OrdinalIgnoreCase)))
                {
                    device.IsBot = true;
                    return device;
                }

                var browser = GetBrowserFromClientHint(headers["Sec-CH-UA"].ToString(), brands, out isGenericBrowser);
                device.BrowserName = browser?.Name;
                device.BrowserVersion = browser?.Version;
            }

            if (headers.ContainsKey("Sec-CH-UA-Platform"))
                device.OSName = MapClientHintPlatform(Unquote(headers["Sec-CH-UA-Platform"].ToString()));

            if (headers.ContainsKey("Sec-CH-UA-Platform-Version"))
                device.OSVersion = Unquote(headers["Sec-CH-UA-Platform-Version"].ToString());

            if (headers.ContainsKey("Sec-CH-UA-Mobile"))
            {
                var value = headers["Sec-CH-UA-Mobile"].ToString();
                if (value == "?1")
                    device.IsMobile = true;
                else if (value == "?0")
                    device.IsMobile = false;
            }

            return device;
        }

        private static string MapClientHintPlatform(string platform)
        {
            if (string.IsNullOrWhiteSpace(platform))
                return null;

            return ClientHintsPlatforms.TryGetValue(platform, out var mapped) ? mapped : platform;
        }

        // Removes quotes around a string value
        private static string Unquote(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.Trim().Trim('"').Trim().NullIfEmpty();
        }

        public virtual Device ParseUserAgent(string userAgent)
        {
            var device = new Device();

            // Check for bots
            if (DetectBot(userAgent))
            {
                device.IsBot = true;
                return device;
            }

            // Parse browser
            if (userAgent.Contains("Edge/"))
            {
                // Edge 12-18 (EdgeHTML)
                device.BrowserName = "Edge";
                device.BrowserVersion = userAgent.Capture(@"Edge/(\d+)");
            }
            else if (userAgent.Contains("Edg/"))
            {
                // Edge 79+ (Chromium)
                device.BrowserName = "Edge";
                device.BrowserVersion = userAgent.Capture(@"Edg/(\d+)");
            }
            else if (userAgent.Contains("UCBrowser/"))
            {
                // UC Browser
                device.BrowserName = "UC Browser";
                device.BrowserVersion = userAgent.Capture(@"UCBrowser/(\d+\.\d+)");
            }
            else if (userAgent.Contains("CriOS/"))
            {
                // Chrome Mobile iOS
                device.BrowserName = "Chrome";
                device.BrowserVersion = userAgent.Capture(@"CriOS/(\d+)");
            }
            else if (userAgent.Contains("FxiOS/"))
            {
                // Firefox Mobile iOS
                device.BrowserName = "Firefox";
                device.BrowserVersion = userAgent.Capture(@"FxiOS/(\d+)");
            }
            else if (userAgent.Contains("Opera Mini/"))
            {
                // Opera Mini
                device.BrowserName = "Opera Mini";
                device.BrowserVersion = userAgent.Capture(@"Opera Mini/(\d+)");
            }
            else if (userAgent.Contains("OPR/"))
            {
                // Opera, Opera Mobile, Opera GX, Opera Touch
                device.BrowserName = "Opera";
                device.BrowserVersion = userAgent.Capture(@"OPR/(\d+)");
            }
            else if (userAgent.Contains("SamsungBrowser/"))
            {
                // Samsung Browser
                device.BrowserName = "Samsung Internet";
                device.BrowserVersion = userAgent.Capture(@"SamsungBrowser/(\d+)");
            }
            else if (userAgent.Contains("Vivaldi/"))
            {
                // Vivaldi
                device.BrowserName = "Vivaldi";
                device.BrowserVersion = userAgent.Capture(@"Vivaldi/(\d+)");
            }
            else if (userAgent.Contains("Brave/"))
            {
                // Brave
                device.BrowserName = "Brave";
                device.BrowserVersion = userAgent.Capture(@"Brave/(\d+)");
            }
            else if (userAgent.Contains("VivoBrowser/"))
            {
                // Vivo
                device.BrowserName = "Vivo";
                device.BrowserVersion = userAgent.Capture(@"VivoBrowser/(\d+)");
            }
            else if (userAgent.Contains("Whale/"))
            {
                // Whale
                device.BrowserName = "Whale";
                device.BrowserVersion = userAgent.Capture(@"Whale/(\d+)");
            }
            else if (userAgent.Contains("YaBrowser/") || userAgent.Contains("Yowser/"))
            {
                // Yandex
                device.BrowserName = "Yandex";
                device.BrowserVersion = userAgent.Capture(@"YaBrowser/(\d+)");
            }
            else if (userAgent.Contains("Yowser/"))
            {
                // Yandex
                device.BrowserName = "Yandex";
                device.BrowserVersion = userAgent.Capture(@"Yowser/(\d+)");
            }
            else if (userAgent.Contains("DuckDuckGo/"))
            {
                // DuckDuckGo Privacy Browser
                device.BrowserName = "DuckDuckGo";
                device.BrowserVersion = userAgent.Capture(@"DuckDuckGo/(\d+)");
            }
            else if (userAgent.Contains("Ecosia"))
            {
                // Ecosia Android/iOS
                device.BrowserName = "Ecosia";
                device.BrowserVersion = userAgent.Capture(@"Ecosia.*?@(\d+)");
            }
            else if (userAgent.Contains("Avast/"))
            {
                // Avast Secure Browser
                device.BrowserName = "Avast";
                device.BrowserVersion = userAgent.Capture(@"Avast/(\d+)");
            }
            else if (userAgent.Contains("Firefox/"))
            {
                // Firefox
                device.BrowserName = "Firefox";
                device.BrowserVersion = userAgent.Capture(@"Firefox/(\d+)");
            }
            else if (userAgent.Contains("Safari/") && userAgent.Contains("Version/"))
            {
                // Safari
                device.BrowserName = "Safari";
                device.BrowserVersion = userAgent.Capture(@"Version/(\d+\.\d+)");
            }
            else if (userAgent.Contains("Chrome/"))
            {
                // Chrome, Chrome Mobile, Chrome WebView, Headless Chrome
                device.BrowserName = "Chrome";
                device.BrowserVersion = userAgent.Capture(@"Chrome/(\d+)");
            }
            else if (userAgent.Contains("MSIE"))
            {
                // Internet Explorer
                device.BrowserName = "Internet Explorer";
                device.BrowserVersion = userAgent.Capture(@"MSIE (\d+)");
            }
            else if (userAgent.Contains("Trident"))
            {
                // Internet Explorer
                device.BrowserName = "Internet Explorer";
                device.BrowserVersion = userAgent.Capture(@"rv:(\d+)");
            }

            // Parse OS
            if (userAgent.Contains("Windows"))
            {
                // Windows
                device.OSName = "Windows";
                device.OSVersion = MapWindowsVersion(userAgent.Capture(@"Windows NT (\d+\.\d+)"));
                device.IsMobile = false;
            }
            else if (userAgent.Contains("Android"))
            {
                // Android
                device.OSName = "Android";
                device.OSVersion = userAgent.Capture(@"Android (\d+)");
                device.IsMobile = true;
            }
            else if (userAgent.Contains("iPhone OS"))
            {
                // iOS
                device.OSName = "iOS";
                device.OSVersion = userAgent.Capture(@"iPhone OS (\d+[_\.]\d+)")?.Replace("_", ".");
                device.IsMobile = true;
            }
            else if (userAgent.Contains("iPad;"))
            {
                // iOS or iPadOS (from version 13)
                device.OSVersion = userAgent.Capture(@"CPU OS (\d+[_\.]\d+)")?.Replace("_", ".");
                device.OSName = decimal.TryParse(device.OSVersion, out decimal v) && v >= 13 ? "iPadOS" : "iOS";
                device.IsMobile = true;
            }
            else if (userAgent.Contains("Macintosh"))
            {
                // Mac
                device.OSName = "Mac";
                device.OSVersion = userAgent.Capture(@"Mac OS X (\d+[_\.]\d+)")?.Replace("_", ".");
                device.IsMobile = false;
            }
            else if (userAgent.Contains("Linux"))
            {
                // Linux
                device.OSName = "Linux";
                device.IsMobile = false;
            }
            else if (userAgent.Contains("CrOS"))
            {
                // Chrome OS
                device.OSName = "Chrome OS";
                device.OSVersion = userAgent.Capture(@"CrOS \w+ (\d+)");
                device.IsMobile = false;
            }
            else if (userAgent.Contains("Android API"))
            {
                // Android
                device.OSName = "Android";
                device.IsMobile = true;
            }

            return device;
        }

        public virtual string MapWindowsVersion(string version)
        {
            return version switch
            {
                "5.1" => "XP",
                "6.0" => "Vista",
                "6.1" => "7",
                "6.2" => "8",
                "6.3" => "8",
                "10.0" => "10",
                _ => null
            };
        }

        private Dictionary<string, string> ParseClientHintUserAgent(string clientHintUserAgent)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(clientHintUserAgent) && clientHintUserAgent.Length <= 500)
            {
                foreach (Match match in clientHintBrandRegex.Matches(clientHintUserAgent))
                {
                    var brand = match.Groups[1].Value.Trim();
                    var version = match.Groups[2].Value.Trim();

                    if (!string.IsNullOrWhiteSpace(brand) && !string.IsNullOrWhiteSpace(version))
                        result[brand] = version;
                }
            }

            return result;
        }

        private Browser GetBrowserFromClientHint(string clientHintUserAgent, Dictionary<string, string> brands, out bool isGeneric)
        {
            isGeneric = false;

            var matches = brands.Keys.Intersect(ClientHintsBrowsers.Keys, StringComparer.OrdinalIgnoreCase).ToList();

            var names = matches.Select(x => ClientHintsBrowsers[x]).Distinct().ToList();
            if (names.Count == 1)
            {
                var name = names.Single();
                var brand = matches.FirstOrDefault(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase)) ?? matches.First();

                return new Browser
                {
                    Name = name,
                    Version = brands[brand]
                };
            }
            else if (names.Count == 0)
            {
                if (brands.TryGetValue("Chromium", out var chromiumVersion))
                {
                    isGeneric = true;
                    return new Browser { Name = "Chrome", Version = chromiumVersion };
                }

                if (ShouldLogClientHintsError(clientHintUserAgent))
                    logger.LogDebug("Could not detect known browser from Sec-CH-UA header: {header} (no match)", clientHintUserAgent);
            }
            else
            {
                if (ShouldLogClientHintsError(clientHintUserAgent))
                    logger.LogDebug("Could not detect known browser from Sec-CH-UA header: {header} ({count} matches)", clientHintUserAgent, names.Count);
            }

            return null;
        }

        // Only log each distinct header once, up to a max count
        private static bool ShouldLogClientHintsError(string clientHintUserAgent)
        {
            return loggedClientHintsErrors.Count < maxLoggedClientHintsErrors && loggedClientHintsErrors.TryAdd(clientHintUserAgent ?? "", 0 /* dummy value */);
        }
    }
}
