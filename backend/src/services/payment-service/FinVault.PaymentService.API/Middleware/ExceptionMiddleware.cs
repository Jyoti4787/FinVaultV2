// ==================================================================
// FILE : ExceptionMiddleware.cs
// LAYER: API (Middleware)
// PATH : payment-service/FinVault.PaymentService.API/Middleware/
//
// WHAT IS THIS?
// This is a "SAFETY NET" for your entire service.
// If your code CRASHES or an error occurs anywhere, this middleware
// catches it BEFORE it reaches the user.
//
// Instead of a scary white page or technical error, it returns 
// a clean JSON: { "success": false, "message": "Oops!" }
// ==================================================================

using System.Net;
using System.Text.Json;

namespace FinVault.PaymentService.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Try to run the next part of the application
            await _next(context);
        }
        catch (Exception ex)
        {
            // If something BROKE, log it for developers to see
            _logger.LogError(ex, "Something went wrong in Payment Service");
            
            // Send a nice response back to the user
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        // 400 = User's fault (Bad Input)
        // 500 = Our fault (Bug in code)
        context.Response.StatusCode = exception switch
        {
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            ArgumentException           => (int)HttpStatusCode.BadRequest,
            _                           => (int)HttpStatusCode.InternalServerError
        };

        var response = new 
        {
            success = false,
            message = exception.Message,
            // Only show details in development, hide in production for security!
            detail  = exception.InnerException?.Message 
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
