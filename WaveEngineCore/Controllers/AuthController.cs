using Microsoft.AspNetCore.Mvc;
using Microwave.Domain.DTOs;
using Microwave.Domain.Interfaces;
using Microwave.Domain.Exceptions;

namespace WaveEngineCore.Controllers;

/// <summary>
/// Controller responsável pela autenticação e autorização da API
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Realiza login na API e retorna um token JWT
    /// </summary>
    /// <param name="credentials">Credenciais de login (username e password)</param>
    /// <returns>Token JWT com informações de expiração</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthCredentials credentials)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return BadRequest(ErrorResponse.CreateValidationError(errors, HttpContext.TraceIdentifier));
            }

            var token = await _authService.AuthenticateAsync(credentials);

            return Ok(new
            {
                success = true,
                message = "Login realizado com sucesso",
                data = new
                {
                    token = token.Token,
                    expiresAt = token.ExpiresAt,
                    username = token.Username
                }
            });
        }
        catch (AuthenticationException ex)
        {
            return Unauthorized(ErrorResponse.CreateAuthenticationError(ex.Message, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante login para usuário: {Username}", credentials.Username);
            return StatusCode(500, ErrorResponse.CreateInternalError(HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Configura as credenciais de acesso iniciais do sistema
    /// </summary>
    /// <param name="request">Dados de configuração (username, password, connection string)</param>
    /// <returns>Confirmação de configuração realizada</returns>
    [HttpPost("configure")]
    public async Task<IActionResult> Configure([FromBody] AuthConfigRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return BadRequest(ErrorResponse.CreateValidationError(errors, HttpContext.TraceIdentifier));
            }

            var success = await _authService.ConfigureAuthAsync(request);

            if (success)
            {
                return Ok(new
                {
                    success = true,
                    message = "Configuração realizada com sucesso"
                });
            }

            return BadRequest(ErrorResponse.CreateBusinessRuleError(
                "Erro ao configurar autenticação",
                "CONFIGURATION_FAILED",
                HttpContext.TraceIdentifier
            ));
        }
        catch (ValidationException ex)
        {
            return BadRequest(ErrorResponse.CreateValidationError(ex.Errors, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante configuração para usuário: {Username}", request.Username);
            return StatusCode(500, ErrorResponse.CreateInternalError(HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Verifica o status de configuração da autenticação
    /// </summary>
    /// <returns>Informações sobre o estado da configuração</returns>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var isConfigured = await _authService.IsConfiguredAsync();
            var settings = isConfigured ? await _authService.GetAuthSettingsAsync() : null;

            return Ok(new
            {
                isConfigured = isConfigured,
                username = settings?.Username,
                lastLoginAt = settings?.LastLoginAt,
                createdAt = settings?.CreatedAt,
                hasConnectionString = !string.IsNullOrEmpty(settings?.ConnectionString)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter status de autenticação");
            return StatusCode(500, ErrorResponse.CreateInternalError(HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateToken([FromBody] TokenValidationRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Token))
            {
                return BadRequest(ErrorResponse.CreateValidationError(
                    new[] { "Token é obrigatório" },
                    HttpContext.TraceIdentifier
                ));
            }

            var isValid = await _authService.ValidateTokenAsync(request.Token);
            var username = isValid ? await _authService.GetUsernameFromTokenAsync(request.Token) : null;

            return Ok(new
            {
                isValid = isValid,
                username = username
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante validação de token");
            return StatusCode(500, ErrorResponse.CreateInternalError(HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new
        {
            success = true,
            message = "Logout realizado com sucesso"
        });
    }
}

public class TokenValidationRequest
{
    public string Token { get; set; } = string.Empty;
}
