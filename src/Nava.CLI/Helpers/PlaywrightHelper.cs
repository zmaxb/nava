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
}