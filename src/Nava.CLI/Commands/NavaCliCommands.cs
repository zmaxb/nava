using CommandDotNet;
using Nava.CLI.Helpers;
using Nava.Core.Utils;

// ReSharper disable UnusedMember.Global

namespace Nava.CLI.Commands;

[Command("nava", Description = "NAVA CLI Loop Commands")]
// ReSharper disable once ClassNeverInstantiated.Global
public class NavaCliCommands
{
    [Command("pw-install", Description = "Install Playwright browsers")]
    public Task<int> InstallPlaywrightBrowsers()
    {
        ConsoleUi.Info("Installing Playwright browsers...");

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            PlaywrightHelper.Install();

            if (cts.IsCancellationRequested)
            {
                ConsoleUi.WriteLine();
                ConsoleUi.Warning("Playwright installation was interrupted.");
                return Task.FromResult(1);
            }

            ConsoleUi.Success("Playwright installation completed.");
            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            ConsoleUi.Error($"Playwright installation failed: {ex.Message}");
            return Task.FromResult(1);
        }
    }


    [Command("pw", Description = "Check if Playwright is installed")]
    public async Task<int> CheckPlaywrightInstalled()
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

    [Command("pw-uninstall", Description = "Uninstall Playwright browsers")]
    public int UninstallPlaywrightBrowsers()
    {
        ConsoleUi.Info("Uninstalling Playwright browsers...");
        PlaywrightHelper.Uninstall();
        return 0;
    }
}