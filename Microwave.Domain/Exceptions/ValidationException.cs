namespace Microwave.Domain.Exceptions;

public class ValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }
    public string ErrorCode { get; }

    public ValidationException(string message, string errorCode = "VALIDATION_ERROR")
        : base(message)
    {
        Errors = new List<string> { message };
        ErrorCode = errorCode;
    }

    public ValidationException(IEnumerable<string> errors, string errorCode = "VALIDATION_ERROR")
        : base(string.Join(", ", errors))
    {
        Errors = errors.ToList();
        ErrorCode = errorCode;
    }

    public ValidationException(string message, IEnumerable<string> errors, string errorCode = "VALIDATION_ERROR")
        : base(message)
    {
        Errors = errors.ToList();
        ErrorCode = errorCode;
    }
}
