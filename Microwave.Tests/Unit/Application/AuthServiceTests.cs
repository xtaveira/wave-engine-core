using Xunit;
using Moq;
using Microwave.Application;
using Microwave.Domain.DTOs;
using Microwave.Domain.Interfaces;
using Microwave.Domain.Exceptions;

namespace Microwave.Tests.Unit.Application;

public class AuthServiceTests
{
    private readonly Mock<IAuthRepository> _mockRepository;
    private readonly Mock<ICryptographyService> _mockCryptography;
    private readonly Mock<IExceptionLogger> _mockLogger;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockRepository = new Mock<IAuthRepository>();
        _mockCryptography = new Mock<ICryptographyService>();
        _mockLogger = new Mock<IExceptionLogger>();

        _authService = new AuthService(_mockRepository.Object, _mockCryptography.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ReturnsAuthToken()
    {
        var credentials = new AuthCredentials { Username = "testuser", Password = "testpass" };
        var authSettings = new AuthSettings
        {
            Username = "testuser",
            PasswordHash = "hashedpassword",
            CreatedAt = DateTime.UtcNow
        };
        var expectedToken = "jwt-token-123";

        _mockRepository.Setup(x => x.GetAuthSettingsAsync()).ReturnsAsync(authSettings);
        _mockCryptography.Setup(x => x.VerifyPassword("testpass", "hashedpassword")).Returns(true);
        _mockCryptography.Setup(x => x.GenerateJwtToken("testuser", It.IsAny<TimeSpan>())).Returns(expectedToken);
        _mockRepository.Setup(x => x.SaveAuthSettingsAsync(It.IsAny<AuthSettings>())).Returns(Task.CompletedTask);

        var result = await _authService.AuthenticateAsync(credentials);

        Assert.Equal(expectedToken, result.Token);
        Assert.Equal("testuser", result.Username);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
        _mockRepository.Verify(x => x.SaveAuthSettingsAsync(It.Is<AuthSettings>(s => s.LastLoginAt.HasValue)), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidUsername_ThrowsAuthenticationException()
    {
        var credentials = new AuthCredentials { Username = "wronguser", Password = "testpass" };
        var authSettings = new AuthSettings
        {
            Username = "testuser",
            PasswordHash = "hashedpassword"
        };

        _mockRepository.Setup(x => x.GetAuthSettingsAsync()).ReturnsAsync(authSettings);
        _mockCryptography.Setup(x => x.VerifyPassword("testpass", "hashedpassword")).Returns(true);

        var exception = await Assert.ThrowsAsync<AuthenticationException>(() =>
            _authService.AuthenticateAsync(credentials));

        Assert.Equal("INVALID_CREDENTIALS", exception.ErrorCode);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidPassword_ThrowsAuthenticationException()
    {
        var credentials = new AuthCredentials { Username = "testuser", Password = "wrongpass" };
        var authSettings = new AuthSettings
        {
            Username = "testuser",
            PasswordHash = "hashedpassword"
        };

        _mockRepository.Setup(x => x.GetAuthSettingsAsync()).ReturnsAsync(authSettings);
        _mockCryptography.Setup(x => x.VerifyPassword("wrongpass", "hashedpassword")).Returns(false);

        var exception = await Assert.ThrowsAsync<AuthenticationException>(() =>
            _authService.AuthenticateAsync(credentials));

        Assert.Equal("INVALID_CREDENTIALS", exception.ErrorCode);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenNotConfigured_ThrowsAuthenticationException()
    {
        var credentials = new AuthCredentials { Username = "testuser", Password = "testpass" };

        _mockRepository.Setup(x => x.GetAuthSettingsAsync()).ReturnsAsync((AuthSettings?)null);

        var exception = await Assert.ThrowsAsync<AuthenticationException>(() =>
            _authService.AuthenticateAsync(credentials));

        Assert.Equal("NOT_CONFIGURED", exception.ErrorCode);
        Assert.Contains("Sistema não configurado", exception.Message);
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData("username", "")]
    [InlineData("", "")]
    public async Task AuthenticateAsync_WithEmptyCredentials_ThrowsAuthenticationException(string? username, string? password)
    {
        var credentials = new AuthCredentials { Username = username ?? "", Password = password ?? "" };

        var exception = await Assert.ThrowsAsync<AuthenticationException>(() =>
            _authService.AuthenticateAsync(credentials));

        Assert.Equal("INVALID_CREDENTIALS", exception.ErrorCode);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ReturnsTrue()
    {
        var token = "valid-jwt-token";

        _mockCryptography.Setup(x => x.ValidateJwtToken(token, out It.Ref<string?>.IsAny)).Returns(true);

        var result = await _authService.ValidateTokenAsync(token);

        Assert.True(result);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidToken_ReturnsFalse()
    {
        var token = "invalid-token";

        _mockCryptography.Setup(x => x.ValidateJwtToken(token, out It.Ref<string?>.IsAny)).Returns(false);

        var result = await _authService.ValidateTokenAsync(token);

        Assert.False(result);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithEmptyToken_ReturnsFalse()
    {
        var result = await _authService.ValidateTokenAsync("");

        Assert.False(result);
        _mockCryptography.Verify(x => x.ValidateJwtToken(It.IsAny<string>(), out It.Ref<string?>.IsAny), Times.Never);
    }

    [Fact]
    public async Task GetUsernameFromTokenAsync_WithValidToken_ReturnsUsername()
    {
        var token = "valid-jwt-token";
        var expectedUsername = "testuser";

        _mockCryptography.Setup(x => x.ValidateJwtToken(token, out It.Ref<string?>.IsAny))
            .Returns((string t, out string? u) => { u = expectedUsername; return true; });

        var result = await _authService.GetUsernameFromTokenAsync(token);

        Assert.Equal(expectedUsername, result);
    }

    [Fact]
    public async Task ConfigureAuthAsync_WithValidRequest_ReturnsTrue()
    {
        var request = new AuthConfigRequest
        {
            Username = "newuser",
            Password = "newpass123",
            ConnectionString = "Server=localhost;Database=Test;"
        };

        _mockCryptography.Setup(x => x.HashPassword("newpass123")).Returns("hashedpassword");
        _mockCryptography.Setup(x => x.EncryptConnectionString(It.IsAny<string>())).Returns("encrypted-connection");
        _mockRepository.Setup(x => x.SaveAuthSettingsAsync(It.IsAny<AuthSettings>())).Returns(Task.CompletedTask);

        var result = await _authService.ConfigureAuthAsync(request);

        Assert.True(result);
        _mockRepository.Verify(x => x.SaveAuthSettingsAsync(It.Is<AuthSettings>(s =>
            s.Username == "newuser" &&
            s.PasswordHash == "hashedpassword" &&
            s.ConnectionString == "encrypted-connection")), Times.Once);
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData("username", "")]
    [InlineData("", "")]
    public async Task ConfigureAuthAsync_WithInvalidRequest_ThrowsValidationException(string username, string password)
    {
        var request = new AuthConfigRequest { Username = username, Password = password };

        await Assert.ThrowsAsync<ValidationException>(() => _authService.ConfigureAuthAsync(request));
    }

    [Fact]
    public async Task IsConfiguredAsync_WhenConfigured_ReturnsTrue()
    {
        _mockRepository.Setup(x => x.ExistsAsync()).ReturnsAsync(true);

        var result = await _authService.IsConfiguredAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task IsConfiguredAsync_WhenNotConfigured_ReturnsFalse()
    {
        _mockRepository.Setup(x => x.ExistsAsync()).ReturnsAsync(false);

        var result = await _authService.IsConfiguredAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task GetAuthSettingsAsync_WithEncryptedConnectionString_ReturnsDecryptedSettings()
    {
        var settings = new AuthSettings
        {
            Username = "testuser",
            PasswordHash = "hash",
            ConnectionString = "encrypted-connection"
        };

        _mockRepository.Setup(x => x.GetAuthSettingsAsync()).ReturnsAsync(settings);
        _mockCryptography.Setup(x => x.DecryptConnectionString("encrypted-connection")).Returns("decrypted-connection");

        var result = await _authService.GetAuthSettingsAsync();

        Assert.NotNull(result);
        Assert.Equal("decrypted-connection", result.ConnectionString);
    }

    [Fact]
    public void GenerateToken_WithUsername_ReturnsToken()
    {
        var username = "testuser";
        var expectedToken = "generated-token";

        _mockCryptography.Setup(x => x.GenerateJwtToken(username, It.IsAny<TimeSpan>())).Returns(expectedToken);

        var result = _authService.GenerateToken(username);

        Assert.Equal(expectedToken, result);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsTrue()
    {
        var token = "valid-token";

        _mockCryptography.Setup(x => x.ValidateJwtToken(token, out It.Ref<string?>.IsAny)).Returns(true);

        var result = _authService.ValidateToken(token);

        Assert.True(result);
    }
}
