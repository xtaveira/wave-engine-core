using Microwave.Domain.DTOs;

namespace Microwave.Domain.Interfaces;

public interface IAuthService
{
    Task<AuthToken> AuthenticateAsync(AuthCredentials credentials);
    Task<bool> ValidateTokenAsync(string token);
    Task<string> GetUsernameFromTokenAsync(string token);
    Task<bool> ConfigureAuthAsync(AuthConfigRequest request);
    Task<bool> IsConfiguredAsync();
    Task<AuthSettings?> GetAuthSettingsAsync();
    string GenerateToken(string username);
    bool ValidateToken(string token);
}

public interface ICryptographyService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    string EncryptConnectionString(string connectionString);
    string DecryptConnectionString(string encryptedConnectionString);
    string GenerateJwtToken(string username, TimeSpan expiration);
    bool ValidateJwtToken(string token, out string? username);
}

public interface IAuthRepository
{
    Task<AuthSettings?> GetAuthSettingsAsync();
    Task SaveAuthSettingsAsync(AuthSettings settings);
    Task<bool> ExistsAsync();
}
