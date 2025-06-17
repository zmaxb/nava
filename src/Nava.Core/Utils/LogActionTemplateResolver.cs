using System.Text.RegularExpressions;
using Nava.Core.Services;

namespace Nava.Core.Utils;

public static partial class LogActionTemplateResolver
{
    public static string Resolve(string input, NavaExecutionContext ctx)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var stub = $"{input}";

        // Replace {script:key}
        input = MyRegex().Replace(input, m => ctx.GetScriptValue(m.Groups[1].Value)?.ToString() ?? stub);

        // Replace {flow:key}
        input = MyRegex1().Replace(input, m => ctx.GetFlowValue(m.Groups[1].Value)?.ToString() ?? stub);

        // Replace {page:key}
        input = MyRegex2().Replace(input, m => ctx.GetPageValue(m.Groups[1].Value)?.ToString() ?? stub);

        return input;
    }

    [GeneratedRegex(@"\{script:([^\}]+)\}")]
    private static partial Regex MyRegex();

    [GeneratedRegex(@"\{flow:([^\}]+)\}")]
    private static partial Regex MyRegex1();

    [GeneratedRegex(@"\{page:([^\}]+)\}")]
    private static partial Regex MyRegex2();
}