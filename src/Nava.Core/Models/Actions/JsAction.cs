using Nava.Core.Enums;
using Nava.Core.Services;
using Nava.Core.Utils;
using Newtonsoft.Json;

namespace Nava.Core.Models.Actions;

public class JsAction : NavaAction
{
    public override NavaActionType Type => NavaActionType.Js;

    [JsonProperty(Required = Required.Always)]
    public string ResultName { get; set; } = string.Empty;

    [JsonProperty(Required = Required.Always)]
    public JsCodeSpec? Script { get; set; }

    public override async Task ExecuteAsync(NavaExecutionContext ctx, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(ResultName))
            throw new InvalidOperationException("JsAction: ResultName is required and cannot be empty.");

        var jsCode = await JsCodeResolver.ResolveJsCodeAsync(Script, ctx.Script.Paths.ScriptCatalog);
        if (string.IsNullOrWhiteSpace(jsCode))
            throw new InvalidOperationException($"JsAction '{ResultName}': Script is empty.");

        var wrappedScript = $"(() => {{ {jsCode} }})()";
        var result = await ctx.Page.EvaluateAsync<object>(wrappedScript);

        if (result is string strResult)
        {
            if (!string.IsNullOrWhiteSpace(strResult))
                ctx.SetPageValue(ResultName, strResult);
        }
        else
        {
            var jsonResult = JsonConvert.SerializeObject(result);
            ctx.SetPageValue(ResultName, jsonResult);
        }
    }
}