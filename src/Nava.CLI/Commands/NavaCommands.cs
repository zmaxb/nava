using System.Diagnostics.CodeAnalysis;
using CommandDotNet;

namespace Nava.CLI.Commands;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class NavaCommands
{
    public RunCommand Run { get; set; } = new();

    [DefaultCommand]
    // ReSharper disable once UnusedMember.Global
    public Task<int> Default(string scriptPath, CancellationToken cancellationToken)
    {
        return Run.Run(scriptPath, cancellationToken);
    }
}