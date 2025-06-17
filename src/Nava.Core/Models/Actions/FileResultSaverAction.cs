using Nava.Core.Enums;
using Nava.Core.Services;
using Nava.Core.Utils;

namespace Nava.Core.Models.Actions;

public class FileResultSaverAction : SaverAction
{
    public override NavaActionType Type => NavaActionType.SaveToFile;
    public string? FilePath { get; set; }

    public override async Task ExecuteAsync(NavaExecutionContext ctx, CancellationToken token = default)
    {
        var scriptPaths = ctx.Script.Paths;
        var stores = GetStores(ctx);

        if (stores.Count == 0) ConsoleUi.Warning($"No data found in selected stores: {string.Join(", ", StoreTypes)}");

        string outputPath;
        if (!string.IsNullOrWhiteSpace(FilePath))
        {
            var directory = Path.GetDirectoryName(FilePath);
            outputPath = !string.IsNullOrEmpty(directory)
                ? FilePath
                : Path.Combine(scriptPaths.ScriptCatalog, FilePath);
        }
        else
        {
            outputPath = Path.Combine(scriptPaths.ScriptCatalog, "flow_results.jsonl");
        }

        var report = BuildReport(ctx, ctx.ExecutionInfo);
        var jsonLine = JsonHelper.Serialize(report);

        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        await using (var stream = new StreamWriter(outputPath, true))
        {
            await stream.WriteLineAsync(jsonLine);
        }

        ConsoleUi.Success($"FlowExecutionReport saved to {outputPath}");
    }
}