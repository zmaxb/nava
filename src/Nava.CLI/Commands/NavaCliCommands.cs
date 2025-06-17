using CommandDotNet;
using Nava.CLI.Helpers;
using Nava.Core.Utils;

namespace Nava.CLI.Commands;

[Command("nava", Description = "NAVA CLI Loop Commands")]
// ReSharper disable once ClassNeverInstantiated.Global
public class NavaCliCommands
{
    [Command("pw-install", Description = "Install Playwright browsers")]
    public Task<int> InstallPlaywrightBrowsers(CancellationToken cancellationToken)
    {
        ConsoleUi.Info("Installing Playwright browsers...");
        Microsoft.Playwright.Program.Main(["install"]);
        ConsoleUi.Success("Playwright installation completed.");

        return Task.FromResult(0);
    }

    [Command("pw", Description = "Check if Playwright is installed")]
    public async Task<int> CheckPlaywrightInstalled(CancellationToken cancellationToken)
    {
        ConsoleUi.Info("Checking Playwright installation...");
        var isInstalled = await PlaywrightHelper.AreBrowsersAvailableAsync();
        if (isInstalled)
            ConsoleUi.Success("Playwright browsers are available and working.");
        else
            ConsoleUi.Error(
                "Playwright browsers are missing or failed to launch. Make sure the .playwright folder exists.");

        return 0;
    }
}