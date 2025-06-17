using Nava.Core.Enums;
using Nava.Core.Services;
using Nava.Core.Utils;
using Newtonsoft.Json;

namespace Nava.Core.Models.Actions;

public class WaitAction : NavaAction
{
    public override NavaActionType Type => NavaActionType.Wait;

    [JsonProperty(Required = Required.Always)]
    public int DurationMs { get; set; }

    public override async Task ExecuteAsync(NavaExecutionContext ctx, CancellationToken token = default)
    {
        if (DurationMs <= 0)
            throw new InvalidOperationException("WaitAction: DurationMs must be greater than zero.");

        ConsoleUi.Info($"Waiting for {DurationMs} ms");
        await Task.Delay(DurationMs, token);
    }
}