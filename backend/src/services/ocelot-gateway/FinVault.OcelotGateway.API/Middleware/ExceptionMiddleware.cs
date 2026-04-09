using System.Net;
using System.Text.Json;

namespace FinVault.OcelotGateway.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
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
        catch (UnauthorizedAccessException ex)
        {
            await WriteResponse(context, 401, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteResponse(context, 404, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await WriteResponse(context, 409, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteResponse(context, 500, "An unexpected error occurred.");
        }
    }

    private static async Task WriteResponse(HttpContext context, int statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new
            {
                success = false,
                message,
                traceId = context.TraceIdentifier
            }));
    }
}
