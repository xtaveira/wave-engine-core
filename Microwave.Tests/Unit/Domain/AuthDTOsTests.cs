using Xunit;
using Microwave.Domain.DTOs;

namespace Microwave.Tests.Unit.Domain;

public class AuthDTOsTests
{
    [Fact]
    public void AuthToken_IsValid_WhenNotExpired_ReturnsTrue()
    {
        var token = new AuthToken
        {
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Username = "testuser"
        };

        Assert.True(token.IsValid);
    }

    [Fact]
    public void AuthToken_IsValid_WhenExpired_ReturnsFalse()
    {
        var token = new AuthToken
        {
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            Username = "testuser"
        };

        Assert.False(token.IsValid);
    }

    [Fact]
    public void ErrorResponse_CreateValidationError_CreatesCorrectResponse()
    {
        var errors = new[] { "Error 1", "Error 2" };
        var requestId = "test-request-id";

        var response = ErrorResponse.CreateValidationError(errors, requestId);

        Assert.Equal("Erro de validação", response.Message);
        Assert.Equal("VALIDATION_ERROR", response.ErrorCode);
        Assert.Equal(requestId, response.RequestId);
        Assert.Equal(errors, response.ValidationErrors);
        Assert.True(response.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public void ErrorResponse_CreateBusinessRuleError_CreatesCorrectResponse()
    {
        var message = "Business rule violation";
        var errorCode = "BUSINESS_ERROR";
        var requestId = "test-request-id";

        var response = ErrorResponse.CreateBusinessRuleError(message, errorCode, requestId);

        Assert.Equal(message, response.Message);
        Assert.Equal(errorCode, response.ErrorCode);
        Assert.Equal(requestId, response.RequestId);
        Assert.True(response.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public void ErrorResponse_CreateAuthenticationError_CreatesCorrectResponse()
    {
        var message = "Authentication failed";
        var requestId = "test-request-id";

        var response = ErrorResponse.CreateAuthenticationError(message, requestId);

        Assert.Equal(message, response.Message);
        Assert.Equal("AUTHENTICATION_FAILED", response.ErrorCode);
        Assert.Equal(requestId, response.RequestId);
        Assert.True(response.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public void ErrorResponse_CreateInternalError_CreatesCorrectResponse()
    {
        var requestId = "test-request-id";

        var response = ErrorResponse.CreateInternalError(requestId);

        Assert.Equal("Erro interno do servidor", response.Message);
        Assert.Equal("INTERNAL_ERROR", response.ErrorCode);
        Assert.Equal(requestId, response.RequestId);
        Assert.True(response.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public void AuthSettings_DefaultValues_AreSetCorrectly()
    {
        var settings = new AuthSettings();

        Assert.Empty(settings.Username);
        Assert.Empty(settings.PasswordHash);
        Assert.Null(settings.ConnectionString);
        Assert.True(settings.CreatedAt <= DateTime.UtcNow);
        Assert.Null(settings.LastLoginAt);
    }
}
