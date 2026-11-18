using System.Text.Json;
using WorkItemApi.Exceptions;

namespace WorkItemApi.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled API exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, code) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "NOT_FOUND"),
            ForbiddenException => (StatusCodes.Status403Forbidden, "FORBIDDEN"),
            ConflictException => (StatusCodes.Status409Conflict, "CONFLICT"),
            ArgumentException => (StatusCodes.Status400BadRequest, "INVALID_ARGUMENT"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "INVALID_OPERATION"),
            _ => (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = new
            {
                code,
                message = exception.Message
            }
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
