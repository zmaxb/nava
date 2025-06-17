using System.Diagnostics;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using Microsoft.Playwright;
using Nava.Core.Enums;
using Nava.Core.Models;

namespace Nava.Core.Factories;

public static class BrowserFactory
{
    public static async Task<(IBrowser, IPage)> LaunchAsync(NavaEnvironment env, IPlaywright playwright)
    {
        switch (env.Browser)
        {
            case NavaBrowserType.Chromium:
                return await LaunchInternalAsync(playwright.Chromium.LaunchAsync, env);
            case NavaBrowserType.Firefox:
                return await LaunchInternalAsync(playwright.Firefox.LaunchAsync, env);
            case NavaBrowserType.Webkit:
                return await LaunchInternalAsync(playwright.Webkit.LaunchAsync, env);
            case NavaBrowserType.ChromiumCdp:
                var wsUrl = await StartSystemChromeAndGetWsUrl(env);
                if (wsUrl == null)
                    throw new Exception("Failed to launch external browser or get WebSocket endpoint.");

                var browser = await playwright.Chromium.ConnectOverCDPAsync(wsUrl);
                var context = browser.Contexts.FirstOrDefault() ?? await browser.NewContextAsync();
                var page = context.Pages.FirstOrDefault() ?? await context.NewPageAsync();
                return (browser, page);
            default:
                throw new NotSupportedException($"Browser type {env.Browser} is not supported.");
        }
    }

    private static async Task<(IBrowser, IPage)> LaunchInternalAsync(
        Func<BrowserTypeLaunchOptions, Task<IBrowser>> launcher,
        NavaEnvironment env)
    {
        var browser = await launcher(new BrowserTypeLaunchOptions
        {
            Headless = env.Headless,
            SlowMo = env.SlowMo
        });
        var context = await browser.NewContextAsync(CreateContextOptions(env));
        var page = await context.NewPageAsync();
        return (browser, page);
    }

    private static BrowserNewContextOptions CreateContextOptions(NavaEnvironment env)
    {
        var options = new BrowserNewContextOptions();

        // Viewport
        if (env.Viewport is { Width: > 0, Height: > 0 })
            options.ViewportSize = new ViewportSize
            {
                Width = env.Viewport.Width,
                Height = env.Viewport.Height
            };
        else
            options.ViewportSize = null;

        // UserAgent
        if (!string.IsNullOrWhiteSpace(env.UserAgent))
            options.UserAgent = env.UserAgent;

        // Proxy
        if (!string.IsNullOrWhiteSpace(env.Proxy))
            options.Proxy = new Proxy { Server = env.Proxy };

        // Locale
        if (!string.IsNullOrWhiteSpace(env.Locale))
            options.Locale = env.Locale;

        // Timezone
        if (!string.IsNullOrWhiteSpace(env.TimezoneId))
            options.TimezoneId = env.TimezoneId;

        // HTTPS errors
        options.IgnoreHTTPSErrors = env.IgnoreHttpsErrors;

        // Downloads
        options.AcceptDownloads = env.AcceptDownloads;

        // HTTP headers
        if (env.ExtraHttpHeaders is { Count: > 0 })
            options.ExtraHTTPHeaders = env.ExtraHttpHeaders;

        // Device scale factor
        if (env.DeviceScaleFactor is { } dsf)
            options.DeviceScaleFactor = (float)dsf;

        // Mobile
        if (env.IsMobile is { } isMobile)
            options.IsMobile = isMobile;

        // Color scheme
        if (!string.IsNullOrWhiteSpace(env.ColorScheme) &&
            Enum.TryParse<ColorScheme>(env.ColorScheme, true, out var scheme))
            options.ColorScheme = scheme;

        return options;
    }

    private static async Task<string?> StartSystemChromeAndGetWsUrl(NavaEnvironment env)
    {
        var cdp = env.ChromiumCdp ?? new ChromiumCdpOptions();
        var userDataDir = cdp.UserDataDir ?? GetDefaultUserDataDir();
        var port = cdp.DebugPort;

        var chromePath = GetChromePath();
        if (chromePath == null)
            throw new Exception("Chrome/Chromium not found in PATH. Установи браузер, либо проверь переменные среды.");

        string args;
        if (cdp.OverrideDefaultArgs && !string.IsNullOrWhiteSpace(cdp.CustomArgs))
        {
            args = cdp.CustomArgs;
        }
        else
        {
            args = $"--remote-debugging-port={port} " +
                   $"--user-data-dir=\"{userDataDir}\" " +
                   "--no-first-run " +
                   "--no-default-browser-check " +
                   "--disable-session-crashed-bubble " +
                   "--disable-restore-session-state " +
                   "about:blank";

            if (!string.IsNullOrWhiteSpace(cdp.CustomArgs))
                args += " " + cdp.CustomArgs;
        }

        var psi = new ProcessStartInfo
        {
            FileName = chromePath,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = true
        };

        Process.Start(psi);

        using var http = new HttpClient();
        for (var i = 0; i < 20; i++)
        {
            try
            {
                var json = await http.GetFromJsonAsync<JsonVersionResponse>($"http://localhost:{port}/json/version");
                if (!string.IsNullOrWhiteSpace(json?.WebSocketDebuggerUrl))
                    return json.WebSocketDebuggerUrl;
            }
            catch
            {
                // ignored
            }

            await Task.Delay(500);
        }

        return null;
    }

    private static string? GetChromePath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var candidates = new[]
            {
                "chrome.exe", "chrome", "msedge.exe", "chromium.exe"
            };

            return candidates.Select(Where).OfType<string>().FirstOrDefault();
        }
        else // Linux/Mac
        {
            var candidates = new[] { "google-chrome", "chromium", "chrome" };
            return candidates.Select(Which).OfType<string>().FirstOrDefault();
        }
    }

    private static string? Which(string name)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = name,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var result = process.StandardOutput.ReadLine();
            process.WaitForExit();
            return string.IsNullOrWhiteSpace(result) ? null : result.Trim();
        }
        catch
        {
            return null;
        }
    }

    private static string? Where(string name)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = name,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var result = process.StandardOutput.ReadLine();
            process.WaitForExit();
            return string.IsNullOrWhiteSpace(result) ? null : result.Trim();
        }
        catch
        {
            return null;
        }
    }

    private static string GetDefaultUserDataDir()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NavaBrowser")
            :
            // Linux/Mac
            "/tmp/nava-chrome";
    }

    private class JsonVersionResponse
    {
        public string? WebSocketDebuggerUrl { get; init; }
    }
}