namespace Nava.Core.Exceptions;

public class FlowCancelledException(string? message = null) : OperationCanceledException(message);