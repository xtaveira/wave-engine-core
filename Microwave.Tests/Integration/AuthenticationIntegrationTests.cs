using Xunit;
using Microsoft.Extensions.Configuration;
using Microwave.Application;
using Microwave.Infrastructure.Services;
using Microwave.Infrastructure.Repositories;
using Microwave.Infrastructure.Logging;
using Microwave.Domain.DTOs;
using Microwave.Domain.Exceptions;

namespace Microwave.Tests.Integration;

public class AuthenticationIntegrationTests : IDisposable
{
    private readonly string _authFilePath;
    private readonly string _logFilePath;
    private readonly AuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthenticationIntegrationTests()
    {
        _authFilePath = Path.Combine(Path.GetTempPath(), $"auth-integration-{Guid.NewGuid()}.json");
        _logFilePath = Path.Combine(Path.GetTempPath(), $"log-integration-{Guid.NewGuid()}.log");

        var configData = new Dictionary<string, string>
        {
            ["Jwt:Secret"] = "TestSecretKey123456789012345678901234567890",
            ["Encryption:Key"] = "TestEncryptionKey123"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var authRepository = new JsonAuthRepository(_authFilePath);
        var cryptographyService = new CryptographyService(_configuration);
        var exceptionLogger = new FileExceptionLogger(_logFilePath);

        _authService = new AuthService(authRepository, cryptographyService, exceptionLogger);
    }

    [Fact]
    public async Task CompleteAuthFlow_ConfigureLoginValidate_WorksEndToEnd()
    {
        var configRequest = new AuthConfigRequest
        {
            Username = "integrationtest",
            Password = "testpassword123",
            ConnectionString = "Server=localhost;Database=IntegrationTest;User=test;Password=testpass;"
        };

        var credentials = new AuthCredentials
        {
            Username = "integrationtest",
            Password = "testpassword123"
        };


        var isConfiguredBefore = await _authService.IsConfiguredAsync();
        Assert.False(isConfiguredBefore);

        var configResult = await _authService.ConfigureAuthAsync(configRequest);
        Assert.True(configResult);

        var isConfiguredAfter = await _authService.IsConfiguredAsync();
        Assert.True(isConfiguredAfter);

        var authToken = await _authService.AuthenticateAsync(credentials);
        Assert.NotNull(authToken);
        Assert.Equal("integrationtest", authToken.Username);
        Assert.NotEmpty(authToken.Token);
        Assert.True(authToken.IsValid);

        var tokenValid = await _authService.ValidateTokenAsync(authToken.Token);
        Assert.True(tokenValid);

        var usernameFromToken = await _authService.GetUsernameFromTokenAsync(authToken.Token);
        Assert.Equal("integrationtest", usernameFromToken);

        var settings = await _authService.GetAuthSettingsAsync();
        Assert.NotNull(settings);
        Assert.Equal("integrationtest", settings.Username);
        Assert.Equal(configRequest.ConnectionString, settings.ConnectionString);
        Assert.NotNull(settings.LastLoginAt);
    }

    [Fact]
    public async Task AuthenticateWithWrongCredentials_ThrowsAuthenticationException()
    {
        var configRequest = new AuthConfigRequest
        {
            Username = "testuser",
            Password = "correctpassword"
        };

        var wrongCredentials = new AuthCredentials
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        await _authService.ConfigureAuthAsync(configRequest);

        var exception = await Assert.ThrowsAsync<AuthenticationException>(() =>
            _authService.AuthenticateAsync(wrongCredentials));

        Assert.Equal("INVALID_CREDENTIALS", exception.ErrorCode);
    }

    [Fact]
    public async Task AuthenticateWithoutConfiguration_ThrowsAuthenticationException()
    {
        if (File.Exists(_authFilePath))
        {
            File.Delete(_authFilePath);
        }

        var credentials = new AuthCredentials
        {
            Username = "testuser",
            Password = "testpassword"
        };

        var exception = await Assert.ThrowsAsync<AuthenticationException>(() =>
            _authService.AuthenticateAsync(credentials));

        Assert.Equal("NOT_CONFIGURED", exception.ErrorCode);
        Assert.Contains("Sistema não configurado", exception.Message);
    }

    [Fact]
    public async Task ReconfigureAuth_UpdatesExistingSettings()
    {
        var initialConfig = new AuthConfigRequest
        {
            Username = "initialuser",
            Password = "initialpass"
        };

        var updatedConfig = new AuthConfigRequest
        {
            Username = "updateduser",
            Password = "updatedpass",
            ConnectionString = "Server=updated;Database=Test;"
        };

        await _authService.ConfigureAuthAsync(initialConfig);
        await _authService.ConfigureAuthAsync(updatedConfig);

        var initialCredentials = new AuthCredentials
        {
            Username = "initialuser",
            Password = "initialpass"
        };

        var updatedCredentials = new AuthCredentials
        {
            Username = "updateduser",
            Password = "updatedpass"
        };

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            _authService.AuthenticateAsync(initialCredentials));

        var token = await _authService.AuthenticateAsync(updatedCredentials);
        Assert.Equal("updateduser", token.Username);

        var settings = await _authService.GetAuthSettingsAsync();
        Assert.Equal("updateduser", settings!.Username);
        Assert.Equal("Server=updated;Database=Test;", settings.ConnectionString);
    }

    [Fact]
    public async Task TokenExpiration_ValidatesCorrectly()
    {
        var configRequest = new AuthConfigRequest
        {
            Username = "tokentest",
            Password = "testpass123"
        };

        var credentials = new AuthCredentials
        {
            Username = "tokentest",
            Password = "testpass123"
        };

        await _authService.ConfigureAuthAsync(configRequest);

        var authToken = await _authService.AuthenticateAsync(credentials);

        Assert.True(authToken.IsValid);
        Assert.True(await _authService.ValidateTokenAsync(authToken.Token));

        var cryptoService = new CryptographyService(_configuration);
        var expiredToken = cryptoService.GenerateJwtToken("tokentest", TimeSpan.FromMilliseconds(1));

        await Task.Delay(10);

        Assert.False(await _authService.ValidateTokenAsync(expiredToken));
    }

    [Fact]
    public async Task ConcurrentAccess_HandlesThreadSafety()
    {
        var configRequest = new AuthConfigRequest
        {
            Username = "concurrenttest",
            Password = "testpass123"
        };

        await _authService.ConfigureAuthAsync(configRequest);

        var credentials = new AuthCredentials
        {
            Username = "concurrenttest",
            Password = "testpass123"
        };

        var tasks = new List<Task<AuthToken>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_authService.AuthenticateAsync(credentials));
        }

        var results = await Task.WhenAll(tasks);

        Assert.Equal(10, results.Length);
        Assert.All(results, token =>
        {
            Assert.NotNull(token);
            Assert.Equal("concurrenttest", token.Username);
            Assert.NotEmpty(token.Token);
            Assert.True(token.IsValid);
        });
    }

    public void Dispose()
    {
        if (File.Exists(_authFilePath))
        {
            File.Delete(_authFilePath);
        }

        if (File.Exists(_logFilePath))
        {
            File.Delete(_logFilePath);
        }
    }
}
