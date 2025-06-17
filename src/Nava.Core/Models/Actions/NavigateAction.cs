using Microsoft.Playwright;
using Nava.Core.Enums;
using Nava.Core.Services;
using Nava.Core.Utils;
using Spectre.Console;

namespace Nava.Core.Models.Actions;

public class NavigateAction : NavaAction
{
    public override NavaActionType Type => NavaActionType.Navigate;

    public string? Url { get; set; }

    public int? Timeout { get; set; } = 30000;

    public override async Task ExecuteAsync(NavaExecutionContext ctx, CancellationToken token = default)
    {
        var urlToGo = !string.IsNullOrWhiteSpace(Url)
            ? Url
            : ctx.CurrentTarget.Url;

        if (string.IsNullOrWhiteSpace(urlToGo))
            throw new InvalidOperationException("No URL provided for NavigateAction. " +
                                                "Specify either 'url' in the action or ensure context provides a URL.");

        try
        {
            ConsoleUi.Info($"Navigate to {urlToGo}");
            var response = await ctx.Page.GotoAsync(urlToGo, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = Timeout
            });

            LogNavigationResult(response);
        }
        catch (PlaywrightException ex) when (ex.Message.Contains("Timeout"))
        {
            AnsiConsole.MarkupLine(
                $"[red]Timeout exceeded while navigating to[/] [underline]{urlToGo}[/] [grey]({Timeout}ms)[/]");
        }
    }

    private void LogNavigationResult(IResponse? response)
    {
        if (response != null)
        {
            if (response.Status is >= 200 and < 300)
                ConsoleUi.Success($"{response.Status} {response.StatusText}");
            else
                ConsoleUi.Error($"{response.Status} {response.StatusText}");
        }
        else
        {
            ConsoleUi.Error("No HTTP response received.");
        }
    }
}