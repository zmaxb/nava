using System.Reflection;
using CommandDotNet;
using Nava.CLI.Commands;
using Nava.CLI.Helpers;
using Nava.Core.Utils;

namespace Nava.CLI;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var version = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        ConsoleUi.Info($"NAVA {version}");

        var browsersAvailable = await PlaywrightHelper.AreBrowsersAvailableAsync();
        if (!browsersAvailable)
        {
            ConsoleUi.Warning("Playwright browsers are missing or failed to launch.");

            Console.Write("Do you want to run 'pw-install' now? [Y/n]: ");
            var answer = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(answer) || answer is "y" or "yes")
            {
                var cli = new NavaCliCommands();
                var result = await cli.InstallPlaywrightBrowsers();

                if (result != 0)
                {
                    ConsoleUi.Error("Playwright installation failed. Aborting.");
                    return result;
                }

                browsersAvailable = await PlaywrightHelper.AreBrowsersAvailableAsync();
                if (!browsersAvailable)
                {
                    ConsoleUi.Error("Browsers still not available after installation. Aborting.");
                    return 1;
                }
            }
            else
            {
                ConsoleUi.Error("Cannot continue without Playwright. Exiting.");
                return 1;
            }
        }

        if (args.Length != 0)
            return await new AppRunner<NavaCommands>()
                .UseDefaultMiddleware()
                .RunAsync(args);

        var cliCommand = new CliCommand();
        return await cliCommand.Execute(CancellationToken.None);
    }
}