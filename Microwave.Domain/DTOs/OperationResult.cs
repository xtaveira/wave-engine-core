namespace Microwave.Domain.DTOs;

public class OperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }

    public static OperationResult CreateSuccess(string message = "")
        => new() { Success = true, Message = message };

    public static OperationResult CreateError(string message, string? errorCode = null)
        => new() { Success = false, Message = message, ErrorCode = errorCode };
}