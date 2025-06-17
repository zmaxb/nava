using System.Diagnostics;
using Microsoft.Playwright;
using Nava.Core.Enums;
using Nava.Core.Exceptions;
using Nava.Core.Factories;
using Nava.Core.Models;
using Nava.Core.Models.Actions;
using Nava.Core.Utils;

namespace Nava.Core.Services;

public class NavaEngine
{
    public async Task RunScriptAsync(NavaScript script, CancellationToken token = default)
    {
        if (script.Context?.Targets is null || script.Context.Targets.Count == 0)
            throw new InvalidOperationException("No URLs found in context");

        using var playwright = await Playwright.CreateAsync();
        var (browser, page) =
            await BrowserFactory.LaunchAsync(script.Environment ?? throw new InvalidOperationException(), playwright);

        BrowserConsoleLogger.AttachConsoleListener(page);

        for (var i = 0; i < script.Context.Targets.Count; i++)
        {
            var target = script.Context.Targets[i];

            ConsoleUi.WriteRule(
                $"Executing: {script.Name ?? "Unnamed Script"} ({i + 1}/{script.Context.Targets.Count})");
            ConsoleUi.Info($"Target: {target.Url}");
            ConsoleUi.WriteLine();

            var ctx = new NavaExecutionContext
            {
                Script = script,
                Browser = browser,
                Page = page,
                CurrentTarget = target
            };

            try
            {
                var watcher = script.Watcher;
                if (watcher?.Enabled == true)
                    await RunWatcherLoopAsync(ctx, watcher.IntervalMs, token);
                else
                    await ExecuteActionsAsync(ctx, token);
            }
            catch (FlowCancelledException ex)
            {
                LogCancelled($"Flow for target '{target.Url}' cancelled: {ex.Message}");
            }
            catch (ScriptCancelledException ex)
            {
                LogError($"Script cancelled: {ex.Message}");
                break;
            }
        }

        await browser.CloseAsync();
    }

    private async Task ExecuteActionsAsync(NavaExecutionContext ctx, CancellationToken token = default)
    {
        ctx.ContextStore.Flow.Clear();

        for (var i = 0; i < ctx.Script.Flow.Count; i++)
        {
            var action = ctx.Script.Flow[i];

            if (action.Type == NavaActionType.Navigate)
                ctx.ContextStore.Page.Clear();

            await action.InitializeAsync(ctx);

            ConsoleUi.Title($"[{i + 1}/{ctx.Script.Flow.Count}]");

            var logEntry = await ExecuteActionWithLoggingAsync(action, ctx, token);
            ctx.ExecutionInfo.Add(logEntry);

            if (ctx.DebugMode) PauseOnDebug();

            ConsoleUi.WriteLine();

            switch (logEntry.Status)
            {
                case ActionStatus.Skipped:
                    LogCancelled($"Action [{action.Type}] was cancelled.");
                    throw new FlowCancelledException("Action skipped, flow cancelled.");
                case ActionStatus.Failed when action.BreakScriptOnError:
                    LogError($"Execution stopped: action [{action.Type}] failed and BreakScriptOnError is set.");
                    throw new ScriptCancelledException("Script stopped due to critical action failure.");
                case ActionStatus.Failed when action.BreakFlowOnError:
                    LogError($"Flow stopped: action [{action.Type}] failed and BreakFlowOnError is set.");
                    throw new FlowCancelledException("Flow stopped due to action failure.");
                case ActionStatus.Success:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (action is not ConditionalJsAction || logEntry.Status != ActionStatus.Failed) continue;

            LogError("Execution stopped due to failed condition in ConditionalJsAction.");
            throw new OperationCanceledException("Stopped by ConditionalJsAction validation.");
        }
    }

    private async Task RunWatcherLoopAsync(NavaExecutionContext ctx, int intervalMs, CancellationToken token = default)
    {
        var interval = intervalMs > 0 ? intervalMs : 50000;

        while (!token.IsCancellationRequested)
        {
            ConsoleUi.Info($"Watcher: Executing flow for target '{ctx.CurrentTarget.Name ?? ctx.CurrentTarget.Url}'");
            try
            {
                await ExecuteActionsAsync(ctx, token);
            }
            catch (OperationCanceledException)
            {
                LogCancelled("Watcher cancelled.");
                break;
            }
            catch (Exception ex)
            {
                LogError($"Watcher error: {ex.Message}");
            }

            await Task.Delay(interval, token);
        }
    }

    private void LogCancelled(string message)
    {
        ConsoleUi.Warning(message);
    }

    private void LogError(string message)
    {
        ConsoleUi.Error(message);
    }

    private async Task<ActionExecutionInfo> ExecuteActionWithLoggingAsync(NavaAction action, NavaExecutionContext ctx,
        CancellationToken token)
    {
        var sw = Stopwatch.StartNew();
        var logEntry = new ActionExecutionInfo
        {
            ActionType = action.Type,
            ActionName = action.Name,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await action.OnPreHostJsExecuteAsync(ctx, token);
            await action.OnPreExecuteAsync(ctx, token);
            await action.ExecuteAsync(ctx, token);
            await action.OnPostExecuteAsync(ctx, token);
            await action.OnPostHostJsExecuteAsync(ctx, token);

            sw.Stop();

            logEntry.Status = ActionStatus.Success;
            logEntry.Duration = sw.Elapsed;
            logEntry.Message = $"Executed successfully in {sw.ElapsedMilliseconds} ms.";

            if (ctx.DebugMode) ConsoleUi.Info($"Action [{action.Type}] executed in {sw.ElapsedMilliseconds} ms.");
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            sw.Stop();

            logEntry.Status = ActionStatus.Skipped;
            logEntry.Duration = sw.Elapsed;
            logEntry.Message = "Action was cancelled.";

            ConsoleUi.Warning($"Action [{action.Type}] was cancelled.");
        }
        catch (Exception ex)
        {
            sw.Stop();

            logEntry.Status = ActionStatus.Failed;
            logEntry.Duration = sw.Elapsed;
            logEntry.Message = ex.Message;
            logEntry.Exception = ex;

            ConsoleUi.Error($"Action [{action.Type}] failed after {sw.ElapsedMilliseconds} ms: {ex.Message}");
        }

        return logEntry;
    }

    private void PauseOnDebug(string? info = null)
    {
        ConsoleUi.Warning("Debug mode! Press any key to continue...");
        if (info != null) ConsoleUi.Warning(info);

        Console.ReadLine();
    }
}