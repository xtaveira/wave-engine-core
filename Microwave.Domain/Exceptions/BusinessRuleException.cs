namespace Microwave.Domain.Exceptions;

public class BusinessRuleException : Exception
{
    public string ErrorCode { get; }
    public string UserMessage { get; }

    public BusinessRuleException(string message, string errorCode = "BUSINESS_RULE_VIOLATION")
        : base(message)
    {
        ErrorCode = errorCode;
        UserMessage = message;
    }

    public BusinessRuleException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        UserMessage = message;
    }

    public BusinessRuleException(string message, string userMessage, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
        UserMessage = userMessage;
    }
}
