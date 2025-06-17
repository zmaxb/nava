using Nava.Core.Models.Actions;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Nava.Core.Models;

public class NavaScript
{
    public ScriptFilePaths Paths { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool DebugMode { get; set; }

    public WatcherSettings? Watcher { get; set; }

    public List<NavaAction> Flow { get; set; } = [];
    public NavaEnvironment? Environment { get; set; }
    public NavaContext? Context { get; set; }
}