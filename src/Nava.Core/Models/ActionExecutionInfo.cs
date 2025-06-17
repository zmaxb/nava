using Nava.Core.Dtos;
using Nava.Core.Enums;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Nava.Core.Models;

public class ActionExecutionInfo
{
    public NavaActionType ActionType { get; set; }
    public string? ActionName { get; set; }
    public ActionStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public Exception? Exception { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ActionExecutionInfoDto ToDto(ActionExecutionInfo entry)
    {
        return new ActionExecutionInfoDto
        {
            ActionType = entry.ActionType,
            ActionName = entry.ActionName,
            Status = entry.Status,
            Message = entry.Message,
            Duration = entry.Duration,
            ExceptionMessage = entry.Exception?.Message ?? "",
            Timestamp = entry.Timestamp
        };
    }
}