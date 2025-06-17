using Nava.Core.Enums;
using Nava.Core.Services;
using Newtonsoft.Json.Linq;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Nava.Core.Models.Actions;

public abstract class SaverAction : NavaAction
{
    public List<EngineStoreType> StoreTypes { get; set; } = [EngineStoreType.Page];

    public bool IncludeActionsLog { get; set; } = false;

    protected Dictionary<string, IReadOnlyDictionary<string, JToken>> GetStores(NavaExecutionContext ctx)
    {
        var dict = new Dictionary<string, IReadOnlyDictionary<string, JToken>>();

        foreach (var type in StoreTypes)
        {
            var storeName = type.ToString().ToLowerInvariant();
            var store = type switch
            {
                EngineStoreType.Flow => ctx.ContextStore.Flow,
                EngineStoreType.Page => ctx.ContextStore.Page,
                EngineStoreType.Script => ctx.ContextStore.Script as IReadOnlyDictionary<string, JToken>,
                _ => null
            };
            if (store is { Count: > 0 })
                dict[storeName] = store;
        }

        return dict;
    }

    protected FlowExecutionReport BuildReport(NavaExecutionContext ctx, List<ActionExecutionInfo> actionLogs)
    {
        return new FlowExecutionReport
        {
            Timestamp = DateTime.UtcNow,
            TargetUrl = ctx.CurrentTarget.Url,
            TargetName = ctx.CurrentTarget.Name,
            Actions = IncludeActionsLog
                ? actionLogs.Select(ActionExecutionInfo.ToDto).ToList()
                : [],
            FlowStatus = CalculateFlowStatus(actionLogs),
            Stores = GetStores(ctx)
        };
    }

    private static FlowStatus CalculateFlowStatus(List<ActionExecutionInfo> actions)
    {
        if (actions.All(a => a.Status == ActionStatus.Success))
            return FlowStatus.Success;
        if (actions.Any(a => a.Status == ActionStatus.Failed))
            return FlowStatus.Failed;
        return actions.Any(a => a.Status == ActionStatus.Skipped)
            ? FlowStatus.Skipped
            : FlowStatus.Partial;
    }
}