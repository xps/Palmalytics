using System;
using System.Collections.Generic;
using System.Linq;
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

        // This maps known values for the Sec-CH-UA header to the values we want to display
        private readonly Dictionary<string, string> ClientHintsBrowsers = new()
        {
            { "Google Chrome", "Chrome" },
            { "Microsoft Edge", "Edge" },
            { "Safari", "Safari" },
            { "Firefox", "Firefox" },
            { "Brave", "Brave" }
        };

        public virtual Device GetDevice(HttpRequest request)
        {
            var headers = request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString());
            return GetDevice(request.Headers["User-Agent"], headers);
        }

        public virtual Device GetDevice(string userAgent, Dictionary<string, string> headers = null)
        {
            if (headers != null && headers.ContainsKey("Sec-CH-UA"))
                return ParseClientHints(headers);

            if (!string.IsNullOrWhiteSpace(userAgent))
                return ParseUserAgent(userAgent);

            return null;
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

        public virtual Device ParseClientHints(Dictionary<string, string> headers)
        {
            var device = new Device();

            if (headers.ContainsKey("Sec-CH-UA"))
            {
                var browser = GetBrowserFromClientHint(headers["Sec-CH-UA"]);
                device.BrowserName = browser?.Name;
                device.BrowserVersion = browser?.Version;
            }

            if (headers.ContainsKey("Sec-CH-UA-Platform"))
                device.OSName = headers["Sec-CH-UA-Platform"];

            if (headers.ContainsKey("Sec-CH-UA-Platform-Version"))
                device.OSVersion = headers["Sec-CH-UA-Platform-Version"];

            if (headers.ContainsKey("Sec-CH-UA-Mobile"))
            {
                var value = headers["Sec-CH-UA-Mobile"];
                if (value == "?1")
                    device.IsMobile = true;
                else if (value == "?0")
                    device.IsMobile = false;
            }

            return device;
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
            var result = new Dictionary<string, string>();

            var parts = clientHintUserAgent.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim());

            foreach (var part in parts)
            {
                var brand = part.Split(';')[0].Trim('"');
                var version = part.Split(';')[1].Capture("v=\"(.+?)\"");
                if (!string.IsNullOrWhiteSpace(brand) && !string.IsNullOrWhiteSpace(version))
                    result[brand] = version;
            }

            return result;
        }

        private Browser GetBrowserFromClientHint(string clientHintUserAgent)
        {
            var brands = ParseClientHintUserAgent(clientHintUserAgent);

            var matches = brands.Keys.Intersect(ClientHintsBrowsers.Keys).ToList();
            if (matches.Count == 1)
            {
                var brand = matches.Single();
                return new Browser
                {
                    Name = MapToDisplayBrowserName(brand),
                    Version = brands[brand]
                };
            }
            else if (matches.Count == 0)
            {
                logger.LogDebug("Could not detect known browser from Sec-CH-UA header: {header} (no match)", clientHintUserAgent);
            }
            else
            {
                logger.LogDebug("Could not detect known browser from Sec-CH-UA header: {header} ({count} matches)", clientHintUserAgent, matches.Count);
            }

            return null;
        }

        private string MapToDisplayBrowserName(string browserName)
        {
            return browserName switch
            {
                "Google Chrome" => "Chrome",
                "Microsoft Edge" => "Edge",
                _ => browserName,
            };
        }
    }
}
