using System.Net;
using System.Text.Json;
using AuthService.Application.Exceptions;

namespace AuthService.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        var (statusCode, code, message) = exception switch
        {
            Application.Exceptions.ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                validationEx.Code,
                validationEx.Message
            ),
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                notFoundEx.Code,
                notFoundEx.Message
            ),
            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                conflictEx.Code,
                conflictEx.Message
            ),
            UnauthorizedException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                unauthorizedEx.Code,
                unauthorizedEx.Message
            ),
            Application.Exceptions.ApplicationException appEx => (
                HttpStatusCode.BadRequest,
                appEx.Code,
                appEx.Message
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "INTERNAL_ERROR",
                "An internal error occurred"
            )
        };

        _logger.LogError(exception, "Error processing request. Code: {Code}, Message: {Message}, CorrelationId: {CorrelationId}",
            code, message, correlationId);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            code,
            message,
            traceId = correlationId
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
