using Nava.Core.Enums;
using Nava.Core.Services;
using Nava.Core.Utils;
using Newtonsoft.Json;
using Spectre.Console;

namespace Nava.Core.Models.Actions;

public class LogAction : NavaAction
{
    public override NavaActionType Type => NavaActionType.Log;

    [JsonProperty(Required = Required.Always)]
    public string Message { get; set; } = null!;

    public string? Style { get; set; }

    public override Task ExecuteAsync(NavaExecutionContext ctx, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(Message))
            throw new InvalidOperationException("LogAction: Message is required and cannot be empty.");

        try
        {
            var resolvedMessage = LogActionTemplateResolver.Resolve(Message, ctx);
            var safeMessage = resolvedMessage.EscapeMarkup();
            var markup = string.IsNullOrWhiteSpace(Style)
                ? safeMessage
                : $"[{Style}]{safeMessage}[/]";

            AnsiConsole.MarkupLine(markup);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Invalid style '{Style}' in LogAction.", ex);
        }

        return Task.CompletedTask;
    }
}