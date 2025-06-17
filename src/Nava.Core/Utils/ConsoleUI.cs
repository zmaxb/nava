using Spectre.Console;

namespace Nava.Core.Utils;

public static class ConsoleUi
{
    public static void Info(string message)
    {
        AnsiConsole.MarkupLine($"[grey][[info]][/] {message.EscapeMarkup()}");
    }

    public static void Success(string message)
    {
        AnsiConsole.MarkupLine($"[green][[ok]][/] {message.EscapeMarkup()}");
    }

    public static void Warning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow][[warn]][/] {message.EscapeMarkup()}");
    }

    public static void Error(string message)
    {
        AnsiConsole.MarkupLine($"[red][[error]][/] {message.EscapeMarkup()}");
    }

    public static void Title(string text)
    {
        AnsiConsole.MarkupLine($"[bold blue]{text.EscapeMarkup()}[/]");
    }

    public static void Log(string message, string label = "log", string color = "cyan")
    {
        AnsiConsole.MarkupLine($"[{color}][[{label}]][/] {message.EscapeMarkup()}");
    }

    public static void WriteLine(string message = "")
    {
        Console.WriteLine(message);
    }

    public static void WriteRule(string text, Justify justification = Justify.Left, string color = "yellow")
    {
        AnsiConsole.Write(
            new Rule($"[{color}]{text}[/]")
            {
                Justification = justification
            }
        );
    }
}