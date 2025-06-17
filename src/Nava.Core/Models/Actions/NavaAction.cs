using Nava.Core.Enums;
using Nava.Core.Services;
using Nava.Core.Utils;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global

// ReSharper disable UnusedMember.Global

namespace Nava.Core.Models.Actions;

public abstract class NavaAction
{
    public abstract NavaActionType Type { get; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string? Name { get; set; }

    // ReSharper disable once VirtualMemberNeverOverridden.Global
    public virtual string Description => string.Empty;

    public bool BreakFlowOnError { get; set; }
    public bool BreakScriptOnError { get; set; }

    public JsCodeSpec? PreJs { get; set; }
    public JsCodeSpec? PostJs { get; set; }
    public JsCodeSpec? PreHostJs { get; set; }
    public JsCodeSpec? PostHostJs { get; set; }

    public NavaActionInitStore InitStore { get; set; } = new();

    public Task InitializeAsync(NavaExecutionContext ctx)
    {
        SetValues(InitStore.Script, ctx.SetScriptValue);
        SetValues(InitStore.Flow, ctx.SetFlowValue);
        SetValues(InitStore.Page, ctx.SetPageValue);

        return Task.CompletedTask;

        void SetValues(Dictionary<string, JToken> source, Action<string, object?> setter)
        {
            foreach (var kv in source)
                setter(kv.Key, kv.Value);
        }
    }

    private async Task ExecuteJsIfNeededAsync(JsCodeSpec? codeSpec, NavaExecutionContext ctx, bool requirePage,
        // ReSharper disable once UnusedParameter.Local
        CancellationToken token)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (requirePage && ctx.Page == null)
            return;

        var jsCode = await JsCodeResolver.ResolveJsCodeAsync(codeSpec, ctx.Script.Paths.ScriptCatalog);
        if (!string.IsNullOrWhiteSpace(jsCode))
            await NavaJsInterop.EvaluateNavaJsAsync(jsCode, ctx);
    }

    public Task OnPreHostJsExecuteAsync(NavaExecutionContext ctx, CancellationToken token = default)
    {
        return ExecuteJsIfNeededAsync(PreHostJs, ctx, false, token);
    }

    public Task OnPostHostJsExecuteAsync(NavaExecutionContext ctx, CancellationToken token = default)
    {
        return ExecuteJsIfNeededAsync(PostHostJs, ctx, false, token);
    }

    public Task OnPreExecuteAsync(NavaExecutionContext ctx, CancellationToken token = default)
    {
        return ExecuteJsIfNeededAsync(PreJs, ctx, true, token);
    }

    public Task OnPostExecuteAsync(NavaExecutionContext ctx, CancellationToken token = default)
    {
        return ExecuteJsIfNeededAsync(PostJs, ctx, true, token);
    }

    public abstract Task ExecuteAsync(NavaExecutionContext ctx, CancellationToken token = default);
}