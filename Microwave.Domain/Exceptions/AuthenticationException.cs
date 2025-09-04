namespace Microwave.Domain.Exceptions;

public class AuthenticationException : Exception
{
    public string ErrorCode { get; }

    public AuthenticationException(string message, string errorCode = "AUTHENTICATION_FAILED")
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public AuthenticationException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
