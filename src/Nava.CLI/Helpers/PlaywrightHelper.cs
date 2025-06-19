using System.Runtime.InteropServices;
using Microsoft.Playwright;

namespace Nava.CLI.Helpers;

public static class PlaywrightHelper
{
    public static async Task<bool> AreBrowsersAvailableAsync()
    {
        try
        {
            var playwright = await Playwright.CreateAsync();
            await using var browser =
                await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void Install()
    {
        Microsoft.Playwright.Program.Main(["install"]);
    }

    public static void Uninstall()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".cache", "ms-playwright");

            if (Directory.Exists(cachePath))
            {
                Directory.Delete(cachePath, true);
                Console.WriteLine($"Playwright browsers uninstalled from: {cachePath}");
            }
            else
            {
                Console.WriteLine($"No Playwright browsers found at: {cachePath}");
            }
        }
        else
        {
            Console.WriteLine("Playwright uninstallation is not implemented for this OS.");
        }
    }
}