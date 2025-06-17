using CommandDotNet;
using Nava.Core.Models;
using Nava.Core.Serialization;
using Nava.Core.Services;
using Nava.Core.Utils;
using Newtonsoft.Json;

namespace Nava.CLI.Commands;

[Command("run", Description = "Run a script from a file path.")]
public class RunCommand
{
    private const int ErrorInvalidPath = 1;
    private const int ErrorParseScript = 2;
    private const int ErrorEmptyScript = 3;
    private const int ErrorMissingEnvironment = 4;
    private const int ErrorMissingContext = 5;

    [DefaultCommand]
    public async Task<int> Run(
        [Operand(Description = "Path to script file")]
        string scriptPath,
        CancellationToken cancellationToken)
    {
        ScriptFilePaths scriptPaths;
        try
        {
            scriptPaths = new ScriptFilePaths(scriptPath);
        }
        catch (ArgumentException ex)
        {
            ConsoleUi.Error($"Invalid script path: {ex.Message}");
            return ErrorInvalidPath;
        }
        catch (Exception ex)
        {
            ConsoleUi.Error($"Unexpected error: {ex.Message}");
            return ErrorInvalidPath;
        }

        ConsoleUi.Info($"{scriptPath}");
        ConsoleUi.WriteLine();

        var (code, script) = await BuildScriptAsync(scriptPaths, cancellationToken);
        if (code > 0) return code;

        if (!string.IsNullOrEmpty(script?.Description)) ConsoleUi.Info($"Description: {script.Description}");

        var engine = new NavaEngine();
        if (script != null) await engine.RunScriptAsync(script, cancellationToken);

        ConsoleUi.WriteRule("Script execution complete");
        return 0;
    }

    private async Task<(int code, NavaScript? script)> BuildScriptAsync(ScriptFilePaths scriptPaths,
        CancellationToken cancellationToken)
    {
        var jsonSettings = new JsonSerializerSettings
        {
            Converters = { new NavaActionJsonConverter() }
        };

        try
        {
            var json = await File.ReadAllTextAsync(scriptPaths.ScriptFile, cancellationToken);
            var script = JsonConvert.DeserializeObject<NavaScript>(json, jsonSettings);

            if (script is null)
            {
                ConsoleUi.Error("Script is empty or invalid");
                return (ErrorEmptyScript, null);
            }

            script.Paths = scriptPaths;

            if (File.Exists(scriptPaths.EnvironmentFile))
                script.Environment =
                    await LoadScriptDataFromFileAsync<NavaEnvironment>(scriptPaths.EnvironmentFile, jsonSettings,
                        cancellationToken);

            if (File.Exists(scriptPaths.ContextFile))
                script.Context =
                    await LoadScriptDataFromFileAsync<NavaContext>(scriptPaths.ContextFile, jsonSettings,
                        cancellationToken);

            if (script.Environment == null)
            {
                ConsoleUi.Error("Script environment is missing.");
                return (ErrorMissingEnvironment, null);
            }

            // ReSharper disable once InvertIf
            if (script.Context == null)
            {
                ConsoleUi.Error("Script context is missing.");
                return (ErrorMissingContext, null);
            }

            return (0, script);
        }
        catch (Exception ex)
        {
            ConsoleUi.Error($"Failed to parse script: {ex.Message}");
            return (ErrorParseScript, null);
        }
    }

    private async Task<T?> LoadScriptDataFromFileAsync<T>(string path, JsonSerializerSettings? settings,
        CancellationToken cancellationToken)
    {
        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return JsonConvert.DeserializeObject<T>(json, settings);
    }
}