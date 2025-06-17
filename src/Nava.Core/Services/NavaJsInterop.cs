using Jint;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nava.Core.Services;

public static class NavaJsInterop
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static async Task<object?> EvaluateNavaJsAsync(string jsCode, NavaExecutionContext ctx)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (ctx.Page != null)
        {
            var injectInit =
                $$"""
                  window._navaStore = window._navaStore || {};
                  window._navaStore.script = {{JsonConvert.SerializeObject(ctx.ContextStore.Script)}};
                  window._navaStore.flow = {{JsonConvert.SerializeObject(ctx.ContextStore.Flow)}};
                  window._navaStore.page = {{JsonConvert.SerializeObject(ctx.ContextStore.Page)}};
                  """;
            await ctx.Page.EvaluateAsync(injectInit);

            var result = await ctx.Page.EvaluateAsync<object?>(jsCode);

            var scriptJson = await ctx.Page.EvaluateAsync<string>("JSON.stringify(window._navaStore.script)");
            var flowJson = await ctx.Page.EvaluateAsync<string>("JSON.stringify(window._navaStore.flow)");
            var pageJson = await ctx.Page.EvaluateAsync<string>("JSON.stringify(window._navaStore.page)");

            ctx.ContextStore.Script = !string.IsNullOrEmpty(scriptJson)
                ? JsonConvert.DeserializeObject<Dictionary<string, JToken>>(scriptJson) ??
                  new Dictionary<string, JToken>()
                : new Dictionary<string, JToken>();

            ctx.ContextStore.Flow = !string.IsNullOrEmpty(flowJson)
                ? JsonConvert.DeserializeObject<Dictionary<string, JToken>>(flowJson) ??
                  new Dictionary<string, JToken>()
                : new Dictionary<string, JToken>();

            ctx.ContextStore.Page = !string.IsNullOrEmpty(pageJson)
                ? JsonConvert.DeserializeObject<Dictionary<string, JToken>>(pageJson) ??
                  new Dictionary<string, JToken>()
                : new Dictionary<string, JToken>();


            return result;
        }

        var engine = new Engine();

        var storeObj = new
        {
            script = ctx.ContextStore.Script,
            flow = ctx.ContextStore.Flow,
            page = ctx.ContextStore.Page
        };
        var storeJson = JsonConvert.SerializeObject(storeObj);
        var injectVars = $"window = {{}}; window._navaStore = {storeJson};";
        engine.Execute(injectVars);

        engine.Execute(jsCode);

        if (engine.Evaluate("window._navaStore").ToObject() is not IDictionary<string, object> newStore) return null;

        UpdateStore(ctx.ContextStore.Script, newStore.TryGetValue("script", out var s) ? s : null);
        UpdateStore(ctx.ContextStore.Flow, newStore.TryGetValue("flow", out var f) ? f : null);
        UpdateStore(ctx.ContextStore.Page, newStore.TryGetValue("page", out var p) ? p : null);

        return null;
    }

    private static void UpdateStore(Dictionary<string, JToken> store, object? data)
    {
        switch (data)
        {
            case JObject jObj:
            {
                foreach (var prop in jObj.Properties())
                    store[prop.Name] = prop.Value;
                break;
            }
            case IDictionary<string, object> dict:
            {
                foreach (var kv in dict)
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    store[kv.Key] = kv.Value != null ? JToken.FromObject(kv.Value) : JValue.CreateNull();
                break;
            }
        }
    }
}