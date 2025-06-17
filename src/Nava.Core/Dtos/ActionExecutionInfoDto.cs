using Nava.Core.Enums;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Nava.Core.Dtos;

public class ActionExecutionInfoDto
{
    public NavaActionType ActionType { get; set; }
    public string? ActionName { get; set; }
    public ActionStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string? ExceptionMessage { get; set; }
    public DateTime Timestamp { get; set; }
}