using Nava.Core.Enums;
using Nava.Core.Services;
using Nava.Core.Utils;
using Newtonsoft.Json;

namespace Nava.Core.Models.Actions;

public class ConditionalJsAction : NavaAction
{
    public override NavaActionType Type => NavaActionType.ConditionalJs;

    [JsonProperty(Required = Required.Always)]
    public JsCodeSpec Script { get; set; } = null!;

    public override async Task ExecuteAsync(NavaExecutionContext ctx, CancellationToken token = default)
    {
        var jsCode = await JsCodeResolver.ResolveJsCodeAsync(Script, ctx.Script.Paths.ScriptCatalog);
        if (string.IsNullOrWhiteSpace(jsCode))
            throw new InvalidOperationException("ConditionalJsAction: Script is empty.");

        var wrappedScript = $"(() => {{ {jsCode} }})()";
        var result = await ctx.Page.EvaluateAsync<bool>(wrappedScript);

        if (!result)
            throw new InvalidOperationException("ConditionalJsAction check failed: JS returned false.");
    }
}