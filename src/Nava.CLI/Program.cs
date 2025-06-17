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
            ConsoleUi.Error(
                "Playwright browsers are missing or failed to launch. Make sure the .playwright folder exists.");

        if (args.Length == 0)
        {
            var cliCommand = new CliCommand();
            return await cliCommand.Execute(CancellationToken.None);
        }

        var result = await new AppRunner<NavaCommands>()
            .UseDefaultMiddleware()
            .RunAsync(args);

        return result;
    }
}