using Microsoft.Playwright;
using Nava.Core.Models;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedMember.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Nava.Core.Services;

public class NavaExecutionContext
{
    public required NavaScript Script { get; init; }
    public bool DebugMode => Script.DebugMode;
    public NavaEnvironment Environment => Script.Environment!;
    public NavaContext Context => Script.Context!;

    public IBrowser Browser { get; init; }
    public IPage Page { get; init; }

    public NavigationTarget CurrentTarget { get; init; }

    public FlowExecutionReport? ExecutionReport { get; set; }
    public List<ActionExecutionInfo> ExecutionInfo { get; } = [];

    #region Stores

    public NavaExecutionContextStore ContextStore { get; } = new();

    public void SetScriptValue(string key, object? value)
    {
        ContextStore.Script[key] = value != null ? JToken.FromObject(value) : JValue.CreateNull();
    }

    public void SetFlowValue(string key, object? value)
    {
        ContextStore.Flow[key] = value != null ? JToken.FromObject(value) : JValue.CreateNull();
    }

    public void SetPageValue(string key, object? value)
    {
        ContextStore.Page[key] = value != null ? JToken.FromObject(value) : JValue.CreateNull();
    }

    public JToken? GetScriptValue(string key)
    {
        return ContextStore.Script.GetValueOrDefault(key);
    }

    public JToken? GetFlowValue(string key)
    {
        return ContextStore.Flow.GetValueOrDefault(key);
    }

    public JToken? GetPageValue(string key)
    {
        return ContextStore.Page.TryGetValue(key, out var val) ? val : null;
    }

    #endregion
}