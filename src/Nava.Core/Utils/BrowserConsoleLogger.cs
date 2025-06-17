using Microsoft.Playwright;

namespace Nava.Core.Utils;

public static class BrowserConsoleLogger
{
    public static void AttachConsoleListener(IPage page)
    {
        page.Console += (_, msg) =>
        {
            var formattedMessage = FormatBrowserConsoleMessage(msg.Type, msg.Text);
            WriteBrowserConsoleMessage(msg.Type, formattedMessage);
        };
    }

    private static string FormatBrowserConsoleMessage(string type, string text)
    {
        return $"[JS {type}] {text}";
    }

    private static void WriteBrowserConsoleMessage(string type, string message)
    {
        switch (type)
        {
            case "error":
                ConsoleUi.Error(message);
                break;
            case "warning":
                ConsoleUi.Warning(message);
                break;
            case "info":
            case "debug":
                ConsoleUi.Info(message);
                break;
            default:
                ConsoleUi.Log(message);
                break;
        }
    }
}