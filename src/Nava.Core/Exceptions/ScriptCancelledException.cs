namespace Nava.Core.Exceptions;

public class ScriptCancelledException(string? message = null) : OperationCanceledException(message);