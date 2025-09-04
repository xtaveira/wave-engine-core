namespace Microwave.Domain.DTOs;

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string RequestId { get; set; } = string.Empty;
    public IEnumerable<string>? ValidationErrors { get; set; }

    public static ErrorResponse CreateValidationError(IEnumerable<string> errors, string requestId = "")
    {
        return new ErrorResponse
        {
            Message = "Erro de validação",
            ErrorCode = "VALIDATION_ERROR",
            ValidationErrors = errors,
            RequestId = requestId
        };
    }

    public static ErrorResponse CreateBusinessRuleError(string message, string errorCode, string requestId = "")
    {
        return new ErrorResponse
        {
            Message = message,
            ErrorCode = errorCode,
            RequestId = requestId
        };
    }

    public static ErrorResponse CreateAuthenticationError(string message, string requestId = "")
    {
        return new ErrorResponse
        {
            Message = message,
            ErrorCode = "AUTHENTICATION_FAILED",
            RequestId = requestId
        };
    }

    public static ErrorResponse CreateInternalError(string requestId = "")
    {
        return new ErrorResponse
        {
            Message = "Erro interno do servidor",
            ErrorCode = "INTERNAL_ERROR",
            RequestId = requestId
        };
    }
}
