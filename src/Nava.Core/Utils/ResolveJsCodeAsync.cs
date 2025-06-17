using Nava.Core.Models;

namespace Nava.Core.Utils;

public static class JsCodeResolver
{
    public static async Task<string> ResolveJsCodeAsync(JsCodeSpec? codeSpec, string scriptBasePath)
    {
        if (codeSpec == null)
            return string.Empty;

        if (!string.IsNullOrEmpty(codeSpec.Inline))
            return codeSpec.Inline;

        if (string.IsNullOrEmpty(codeSpec.File)) return string.Empty;

        var path = Path.Combine(scriptBasePath, codeSpec.File);
        if (!File.Exists(path))
            throw new FileNotFoundException($"JS file not found: {path}");
        return await File.ReadAllTextAsync(path);
    }
}