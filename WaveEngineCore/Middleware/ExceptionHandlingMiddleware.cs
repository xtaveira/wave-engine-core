using System.Net;
using System.Text.Json;
using Microwave.Domain.DTOs;
using Microwave.Domain.Exceptions;
using Microwave.Domain.Interfaces;

namespace WaveEngineCore.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Uma exceção não tratada ocorreu: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var requestId = context.TraceIdentifier;
        var userId = context.User?.Identity?.Name;

        var exceptionLogger = context.RequestServices.GetRequiredService<IExceptionLogger>();
        await exceptionLogger.LogExceptionAsync(exception, requestId, userId, new Dictionary<string, object>
        {
            ["RequestPath"] = context.Request.Path.Value ?? "",
            ["RequestMethod"] = context.Request.Method,
            ["UserAgent"] = context.Request.Headers.UserAgent.ToString(),
            ["RemoteIpAddress"] = context.Connection.RemoteIpAddress?.ToString() ?? ""
        });

        context.Response.ContentType = "application/json";

        var (statusCode, errorResponse) = exception switch
        {
            AuthenticationException authEx => (
                HttpStatusCode.Unauthorized,
                ErrorResponse.CreateAuthenticationError(authEx.Message, requestId)
            ),

            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                ErrorResponse.CreateValidationError(validationEx.Errors, requestId)
            ),

            BusinessRuleException businessEx => (
                HttpStatusCode.BadRequest,
                ErrorResponse.CreateBusinessRuleError(businessEx.UserMessage, businessEx.ErrorCode, requestId)
            ),

            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                ErrorResponse.CreateValidationError(new[] { argEx.Message }, requestId)
            ),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                ErrorResponse.CreateAuthenticationError("Acesso não autorizado", requestId)
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                ErrorResponse.CreateInternalError(requestId)
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
