using Microwave.Domain.DTOs;
using Microwave.Domain.Exceptions;
using Microwave.Domain.Interfaces;

namespace Microwave.Application;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly ICryptographyService _cryptographyService;
    private readonly IExceptionLogger _logger;

    public AuthService(
        IAuthRepository authRepository,
        ICryptographyService cryptographyService,
        IExceptionLogger logger)
    {
        _authRepository = authRepository;
        _cryptographyService = cryptographyService;
        _logger = logger;
    }

    public async Task<AuthToken> AuthenticateAsync(AuthCredentials credentials)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(credentials.Username) || string.IsNullOrWhiteSpace(credentials.Password))
            {
                throw new AuthenticationException("Nome de usuário e senha são obrigatórios", "INVALID_CREDENTIALS");
            }

            var authSettings = await _authRepository.GetAuthSettingsAsync();
            if (authSettings == null)
            {
                throw new AuthenticationException("Sistema não configurado. Configure as credenciais primeiro.", "NOT_CONFIGURED");
            }

            var isValidPassword = _cryptographyService.VerifyPassword(credentials.Password, authSettings.PasswordHash);
            var isValidUsername = authSettings.Username.Equals(credentials.Username, StringComparison.OrdinalIgnoreCase);

            if (!isValidUsername || !isValidPassword)
            {
                throw new AuthenticationException("Credenciais inválidas", "INVALID_CREDENTIALS");
            }

            var token = _cryptographyService.GenerateJwtToken(credentials.Username, TimeSpan.FromHours(8));

            authSettings.LastLoginAt = DateTime.UtcNow;
            await _authRepository.SaveAuthSettingsAsync(authSettings);

            return new AuthToken
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(8),
                Username = credentials.Username
            };
        }
        catch (AuthenticationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _logger.LogExceptionAsync(ex, additionalData: new Dictionary<string, object>
            {
                ["Username"] = credentials.Username,
                ["Operation"] = "Authentication"
            });
            throw new AuthenticationException("Erro interno durante autenticação", "INTERNAL_ERROR", ex);
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            return _cryptographyService.ValidateJwtToken(token, out _);
        }
        catch (Exception ex)
        {
            await _logger.LogExceptionAsync(ex, additionalData: new Dictionary<string, object>
            {
                ["Operation"] = "TokenValidation"
            });
            return false;
        }
    }

    public async Task<string> GetUsernameFromTokenAsync(string token)
    {
        try
        {
            if (_cryptographyService.ValidateJwtToken(token, out var username))
            {
                return username ?? string.Empty;
            }
            return string.Empty;
        }
        catch (Exception ex)
        {
            await _logger.LogExceptionAsync(ex, additionalData: new Dictionary<string, object>
            {
                ["Operation"] = "GetUsernameFromToken"
            });
            return string.Empty;
        }
    }

    public async Task<bool> ConfigureAuthAsync(AuthConfigRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ValidationException("Nome de usuário e senha são obrigatórios", "INVALID_CONFIGURATION");
            }

            var passwordHash = _cryptographyService.HashPassword(request.Password);
            var encryptedConnectionString = !string.IsNullOrEmpty(request.ConnectionString)
                ? _cryptographyService.EncryptConnectionString(request.ConnectionString)
                : null;

            var authSettings = new AuthSettings
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                ConnectionString = encryptedConnectionString,
                CreatedAt = DateTime.UtcNow
            };

            await _authRepository.SaveAuthSettingsAsync(authSettings);
            return true;
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _logger.LogExceptionAsync(ex, additionalData: new Dictionary<string, object>
            {
                ["Username"] = request.Username,
                ["Operation"] = "Configuration"
            });
            return false;
        }
    }

    public async Task<bool> IsConfiguredAsync()
    {
        try
        {
            return await _authRepository.ExistsAsync();
        }
        catch (Exception ex)
        {
            await _logger.LogExceptionAsync(ex, additionalData: new Dictionary<string, object>
            {
                ["Operation"] = "IsConfigured"
            });
            return false;
        }
    }

    public async Task<AuthSettings?> GetAuthSettingsAsync()
    {
        try
        {
            var settings = await _authRepository.GetAuthSettingsAsync();
            if (settings != null && !string.IsNullOrEmpty(settings.ConnectionString))
            {
                settings.ConnectionString = _cryptographyService.DecryptConnectionString(settings.ConnectionString);
            }
            return settings;
        }
        catch (Exception ex)
        {
            await _logger.LogExceptionAsync(ex, additionalData: new Dictionary<string, object>
            {
                ["Operation"] = "GetAuthSettings"
            });
            return null;
        }
    }

    public string GenerateToken(string username)
    {
        return _cryptographyService.GenerateJwtToken(username, TimeSpan.FromHours(8));
    }

    public bool ValidateToken(string token)
    {
        return _cryptographyService.ValidateJwtToken(token, out _);
    }
}
