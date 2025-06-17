using System.Text;
using Nava.Core.Enums;
using Nava.Core.Services;
using Nava.Core.Utils;

namespace Nava.Core.Models.Actions;

public class PostResultSaverAction : SaverAction
{
    public override NavaActionType Type => NavaActionType.PostResult;
    public string TargetUrl { get; set; } = "";

    public override async Task ExecuteAsync(NavaExecutionContext ctx, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(TargetUrl))
        {
            ConsoleUi.Error("Target URL is missing.");
            return;
        }

        var stores = GetStores(ctx);
        if (stores.Count == 0) ConsoleUi.Warning($"No data found in selected stores: {string.Join(", ", StoreTypes)}");

        var report = BuildReport(ctx, ctx.ExecutionInfo);

        var jsonContent = JsonHelper.Serialize(report);

        using var httpClient = new HttpClient();

        try
        {
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(TargetUrl, content, token);
            if (response.IsSuccessStatusCode)
                ConsoleUi.Success("FlowExecutionReport successfully POSTed to " + TargetUrl);
            else
                ConsoleUi.Warning($"POST to {TargetUrl} failed with status {response.StatusCode}");
        }
        catch (Exception ex)
        {
            ConsoleUi.Error($"Error while POSTing FlowExecutionReport: {ex.Message}");
        }
    }
}