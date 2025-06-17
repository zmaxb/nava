using CommandDotNet;

namespace Nava.CLI.Commands;

[Command("cli", Description = "Start interactive CLI mode")]
public class CliCommand
{
    public async Task<int> Execute(CancellationToken cancellationToken)
    {
        Console.WriteLine("Entering CLI loop mode. Type 'exit' to quit. Type 'help' to show available commands.");

        var appRunner = new AppRunner<NavaCliCommands>()
            .UseDefaultMiddleware();

        while (!cancellationToken.IsCancellationRequested)
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) continue;

            var trimmed = input.Trim();
            if (trimmed.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

            if (trimmed.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                await appRunner.RunAsync("--help");
                continue;
            }

            var args = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            try
            {
                await appRunner.RunAsync(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        Console.WriteLine("Exiting CLI loop mode.");
        return 0;
    }
}