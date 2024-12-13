using HtmlAgilityPack;
using LyricsScraperNET.Network.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Playwright;
using System.Collections.Generic;

namespace LyricsScraperNET.Network
{
    internal sealed class LyricFindWebBrowser : IWebClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LyricFindWebBrowser> _logger;

        public LyricFindWebBrowser()
        {
            _httpClient = new HttpClient();
            var exitCode = Microsoft.Playwright.Program.Main(new[] { "install" });
            if (exitCode != 0)
            {
                throw new Exception($"Playwright exited with code {exitCode}");
            }
        }

        public LyricFindWebBrowser(ILogger<LyricFindWebBrowser> logger) : this()
        {
            _logger = logger;
        }

        public string Load(Uri uri)
        {
            return LoadAsync(uri).GetAwaiter().GetResult();
        }

        // Asynchronous method to load the URI and scrape the WAF token
        public async Task<string> LoadAsync(Uri uri)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            // Set headers to mimic a real browser
            await page.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
            {
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36" },
                { "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8" },
                { "Accept-Language", "en-US,en;q=0.9" },
                { "Accept-Encoding", "gzip, deflate, br" },
                { "Connection", "keep-alive" },
                { "Upgrade-Insecure-Requests", "1" },
                { "Sec-Fetch-Dest", "document" },
                { "Sec-Fetch-Mode", "navigate" },
                { "Sec-Fetch-Site", "none" }
            });

            // Log page console output for debugging
            page.Console += (sender, e) =>
            {
                _logger?.LogDebug($"Console: {e.Text}");
            };

            try
            {
                var response = await page.GotoAsync(uri.ToString(), new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded, // Wait until network activity is idle
                });

                if (response.Status == 202)
                {
                    _logger?.LogInformation($"Received 202 Accepted for URI: {uri}. Executing JavaScript to fetch WAF token.");

                    await page.WaitForFunctionAsync("window.AwsWafIntegration !== undefined", new PageWaitForFunctionOptions
                    {
                        Timeout = 5000
                    });

                    var token = await page.EvaluateAsync<string>(@"
                    AwsWafIntegration.checkForceRefresh().then(forceRefresh => {
                        if (forceRefresh) {
                            return AwsWafIntegration.forceRefreshToken().then(() => {
                                return AwsWafIntegration.getToken();
                            });
                        } else {
                            return AwsWafIntegration.getToken();
                        }
                    });
                ");

                    if (string.IsNullOrEmpty(token))
                    {
                        _logger?.LogWarning("Token could not be retrieved from JavaScript execution.");
                        return string.Empty;
                    }

                    _logger?.LogInformation($"Token successfully retrieved: {token}");

                    await context.AddCookiesAsync(new[] {new Microsoft.Playwright.Cookie
                    {
                        Name = "aws-waf-token",
                        Value = token,
                        Url = uri.ToString()
                    }});

                    response = await page.GotoAsync(uri.ToString());

                    if (!response.Ok)
                    {
                        _logger?.LogWarning($"Follow-up request failed. Status: {response.Status}");
                        return string.Empty;
                    }

                    return await response.TextAsync();
                }

                return await response.TextAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error occurred while scraping: {ex.Message}");
                return string.Empty;
            }
            finally
            {
                await browser.CloseAsync();
            }
        }

        private void CheckDocument(HtmlDocument document, Uri uri)
        {
            if (document == null || string.IsNullOrEmpty(document.ParsedText))
            {
                Console.WriteLine($"Document is invalid for URI: {uri}");
            }
        }
    }
}
