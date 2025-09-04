using Xunit;
using Microsoft.Extensions.Configuration;
using Moq;
using Microwave.Infrastructure.Services;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Microwave.Tests.Unit.Infrastructure;

public class CryptographyServiceTests
{
    private readonly CryptographyService _service;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public CryptographyServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(x => x["Jwt:Secret"]).Returns("TestSecretKey123456789012345678901234567890");
        _mockConfiguration.Setup(x => x["Encryption:Key"]).Returns("TestEncryptionKey123");

        _service = new CryptographyService(_mockConfiguration.Object);
    }

    [Fact]
    public void HashPassword_WithSamePassword_ReturnsConsistentHash()
    {
        var password = "testpassword123";

        var hash1 = _service.HashPassword(password);
        var hash2 = _service.HashPassword(password);

        Assert.Equal(hash1, hash2);
        Assert.NotEmpty(hash1);
    }

    [Fact]
    public void HashPassword_WithDifferentPasswords_ReturnsDifferentHashes()
    {
        var password1 = "testpassword123";
        var password2 = "differentpassword456";

        var hash1 = _service.HashPassword(password1);
        var hash2 = _service.HashPassword(password2);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        var password = "testpassword123";
        var hash = _service.HashPassword(password);

        var result = _service.VerifyPassword(password, hash);

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        var correctPassword = "testpassword123";
        var incorrectPassword = "wrongpassword456";
        var hash = _service.HashPassword(correctPassword);

        var result = _service.VerifyPassword(incorrectPassword, hash);

        Assert.False(result);
    }

    [Fact]
    public void EncryptConnectionString_WithValidString_ReturnsEncryptedValue()
    {
        var connectionString = "Server=localhost;Database=TestDB;User=test;Password=test123;";

        var encrypted = _service.EncryptConnectionString(connectionString);

        Assert.NotEmpty(encrypted);
        Assert.NotEqual(connectionString, encrypted);
    }

    [Fact]
    public void DecryptConnectionString_WithEncryptedString_ReturnsOriginalValue()
    {
        var originalConnectionString = "Server=localhost;Database=TestDB;User=test;Password=test123;";
        var encrypted = _service.EncryptConnectionString(originalConnectionString);

        var decrypted = _service.DecryptConnectionString(encrypted);

        Assert.Equal(originalConnectionString, decrypted);
    }

    [Fact]
    public void EncryptConnectionString_WithEmptyString_ReturnsEmptyString()
    {
        var connectionString = "";

        var encrypted = _service.EncryptConnectionString(connectionString);

        Assert.Empty(encrypted);
    }

    [Fact]
    public void DecryptConnectionString_WithEmptyString_ReturnsEmptyString()
    {
        var encryptedString = "";

        var decrypted = _service.DecryptConnectionString(encryptedString);

        Assert.Empty(decrypted);
    }

    [Fact]
    public void GenerateJwtToken_WithUsername_ReturnsValidToken()
    {
        var username = "testuser";
        var expiration = TimeSpan.FromHours(1);

        var token = _service.GenerateJwtToken(username, expiration);

        Assert.NotEmpty(token);
    }

    [Fact]
    public void ValidateJwtToken_WithValidToken_ReturnsTrueAndUsername()
    {
        var username = "testuser";
        var expiration = TimeSpan.FromHours(1);
        var token = _service.GenerateJwtToken(username, expiration);

        var isValid = _service.ValidateJwtToken(token, out var extractedUsername);

        Assert.True(isValid);
        Assert.Equal(username, extractedUsername);
    }

    [Fact]
    public void ValidateJwtToken_WithInvalidToken_ReturnsFalse()
    {
        var invalidToken = "invalid.token.here";

        var isValid = _service.ValidateJwtToken(invalidToken, out var username);

        Assert.False(isValid);
        Assert.Null(username);
    }

    [Fact]
    public void ValidateJwtToken_WithMalformedToken_ReturnsFalse()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-secret-key-for-jwt-validation"
            })
            .Build();

        var service = new CryptographyService(configuration);
        var invalidToken = "invalid.jwt.token";

        var result = service.ValidateJwtToken(invalidToken, out string? username);

        Assert.False(result);
        Assert.Null(username);
    }
}
