using Microsoft.Playwright;
using Nava.Core.Enums;
using Nava.Core.Services;
using Nava.Core.Utils;

namespace Nava.Core.Models.Actions;

public class ScreenshotAction : NavaAction
{
    public override NavaActionType Type => NavaActionType.Screenshot;

    public string? Path { get; set; }
    public bool FullPage { get; set; } = true;

    public override async Task ExecuteAsync(NavaExecutionContext ctx, CancellationToken token = default)
    {
        try
        {
            var screenshotPath = GetScreenshotPath(ctx);

            var directory = System.IO.Path.GetDirectoryName(screenshotPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            await ctx.Page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = screenshotPath,
                FullPage = FullPage
            });

            ConsoleUi.Success($"Screenshot saved to {screenshotPath}");
        }
        catch (PlaywrightException ex)
        {
            ConsoleUi.Error($"ScreenshotAction error: {ex.Message}");
        }
    }

    private string GetScreenshotPath(NavaExecutionContext ctx)
    {
        if (!string.IsNullOrWhiteSpace(Path) && System.IO.Path.IsPathRooted(Path))
            return Path!;

        var scriptDir = System.IO.Path.GetDirectoryName(ctx.Script.Paths.ScriptFile) ?? Directory.GetCurrentDirectory();

        var screenshotsDir = System.IO.Path.Combine(scriptDir, "screenshots");

        var fileName = string.IsNullOrWhiteSpace(Path)
            ? $"{DateTime.Now:yyyyMMdd_HHmmss_fff}.png"
            : System.IO.Path.GetFileName(Path);

        return System.IO.Path.Combine(screenshotsDir, fileName);
    }
}