// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Nava.Core.Models;

public class ScriptFilePaths
{
    public ScriptFilePaths(string scriptFile,
        string? contextFile = null, string? environmentFile = null)
    {
        ScriptFile = scriptFile ?? throw new ArgumentNullException(nameof(scriptFile));

        ScriptCatalog = Path.GetDirectoryName(scriptFile)
                        ?? throw new ArgumentException("Invalid script file path", nameof(scriptFile));

        ContextFile = !string.IsNullOrWhiteSpace(contextFile)
            ? contextFile
            : Path.Combine(ScriptCatalog, "context.json");

        EnvironmentFile = !string.IsNullOrWhiteSpace(environmentFile)
            ? environmentFile
            : Path.Combine(ScriptCatalog, "environment.json");
    }

    public string ScriptFile { get; set; }
    public string ScriptCatalog { get; set; }
    public string ContextFile { get; set; }
    public string EnvironmentFile { get; set; }
}