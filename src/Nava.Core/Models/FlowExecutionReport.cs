using Nava.Core.Dtos;
using Nava.Core.Enums;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Nava.Core.Models;

public class FlowExecutionReport
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? TargetUrl { get; set; }
    public string? TargetName { get; set; }
    public List<ActionExecutionInfoDto> Actions { get; set; } = [];
    public FlowStatus FlowStatus { get; set; }
    public Dictionary<string, IReadOnlyDictionary<string, JToken>>? Stores { get; set; }
}