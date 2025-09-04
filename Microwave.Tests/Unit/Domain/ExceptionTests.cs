using Xunit;
using Microwave.Domain.Exceptions;

namespace Microwave.Tests.Unit.Domain;

public class ExceptionTests
{
    [Fact]
    public void BusinessRuleException_WithMessage_SetsPropertiesCorrectly()
    {
        var message = "Business rule violated";
        var errorCode = "BUSINESS_ERROR";

        var exception = new BusinessRuleException(message, errorCode);

        Assert.Equal(message, exception.Message);
        Assert.Equal(errorCode, exception.ErrorCode);
        Assert.Equal(message, exception.UserMessage);
    }

    [Fact]
    public void BusinessRuleException_WithUserMessage_SetsPropertiesCorrectly()
    {
        var message = "Internal business rule message";
        var userMessage = "User friendly message";
        var errorCode = "BUSINESS_ERROR";

        var exception = new BusinessRuleException(message, userMessage, errorCode);

        Assert.Equal(message, exception.Message);
        Assert.Equal(userMessage, exception.UserMessage);
        Assert.Equal(errorCode, exception.ErrorCode);
    }

    [Fact]
    public void ValidationException_WithSingleError_SetsPropertiesCorrectly()
    {
        var message = "Validation failed";
        var errorCode = "VALIDATION_ERROR";

        var exception = new ValidationException(message, errorCode);

        Assert.Equal(message, exception.Message);
        Assert.Equal(errorCode, exception.ErrorCode);
        Assert.Single(exception.Errors);
        Assert.Contains(message, exception.Errors);
    }

    [Fact]
    public void ValidationException_WithMultipleErrors_SetsPropertiesCorrectly()
    {
        var errors = new[] { "Error 1", "Error 2", "Error 3" };
        var errorCode = "VALIDATION_ERROR";

        var exception = new ValidationException(errors, errorCode);

        Assert.Equal(string.Join(", ", errors), exception.Message);
        Assert.Equal(errorCode, exception.ErrorCode);
        Assert.Equal(3, exception.Errors.Count);
        Assert.Contains("Error 1", exception.Errors);
        Assert.Contains("Error 2", exception.Errors);
        Assert.Contains("Error 3", exception.Errors);
    }

    [Fact]
    public void AuthenticationException_WithMessage_SetsPropertiesCorrectly()
    {
        var message = "Authentication failed";
        var errorCode = "AUTH_ERROR";

        var exception = new AuthenticationException(message, errorCode);

        Assert.Equal(message, exception.Message);
        Assert.Equal(errorCode, exception.ErrorCode);
    }

    [Fact]
    public void AuthenticationException_WithInnerException_SetsPropertiesCorrectly()
    {
        var message = "Authentication failed";
        var errorCode = "AUTH_ERROR";
        var innerException = new InvalidOperationException("Inner error");

        var exception = new AuthenticationException(message, errorCode, innerException);

        Assert.Equal(message, exception.Message);
        Assert.Equal(errorCode, exception.ErrorCode);
        Assert.Equal(innerException, exception.InnerException);
    }
}
