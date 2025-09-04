using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microwave.Domain.Interfaces;

namespace WaveEngineCore.Middleware;

public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtAuthenticationMiddleware> _logger;

    public JwtAuthenticationMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<JwtAuthenticationMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        var publicPaths = new[]
        {
            "/api/auth/login",
            "/api/auth/configure",
            "/api/auth/status",
            "/error",
            "/settings",
            "/",
            "/privacy"
        };

        var isPublicPath = publicPaths.Any(p => path.StartsWith(p)) ||
                          path.Contains("css") ||
                          path.Contains("js") ||
                          path.Contains("favicon");

        if (isPublicPath)
        {
            await _next(context);
            return;
        }

        var isConfigured = await authService.IsConfiguredAsync();
        if (!isConfigured)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Sistema não configurado. Configure as credenciais em /settings");
            return;
        }

        var token = ExtractTokenFromRequest(context);

        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Token de acesso é obrigatório");
            return;
        }

        var isValidToken = await authService.ValidateTokenAsync(token);
        if (!isValidToken)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Token inválido ou expirado");
            return;
        }

        var username = await authService.GetUsernameFromTokenAsync(token);
        if (!string.IsNullOrEmpty(username))
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim("username", username)
            };

            var identity = new ClaimsIdentity(claims, "jwt");
            context.User = new ClaimsPrincipal(identity);
        }

        await _next(context);
    }

    private string? ExtractTokenFromRequest(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }

        return context.Request.Query["token"].FirstOrDefault();
    }
}
