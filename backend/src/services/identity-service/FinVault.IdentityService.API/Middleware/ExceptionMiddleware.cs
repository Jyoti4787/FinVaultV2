using System.Net;
using System.Text.Json;

namespace FinVault.IdentityService.API.Middleware;

// WHAT IS MIDDLEWARE?
// Think of middleware like a SECURITY CHECKPOINT
// Every single request that comes in
// passes through this checkpoint
//
// Normal flow without middleware:
// Request → Controller → Response
//
// Flow with middleware:
// Request → Middleware → Controller → Middleware → Response
//
// This specific middleware catches CRASHES
// If anything in the controller or handler crashes
// Instead of showing an ugly error page
// We catch it here and return a clean JSON response
//
// WHY catch different exception types?
// UnauthorizedAccessException = 401 (wrong password etc)
// KeyNotFoundException        = 404 (user not found etc)
// InvalidOperationException   = 409 (email already exists etc)
// Everything else             = 500 (unexpected crash)

// Registered in : Program.cs
// Wraps         : Every single HTTP request
public class ExceptionMiddleware
{
    // _next = the next thing in the pipeline
    // When we call _next(context) it continues
    // to the controller
    private readonly RequestDelegate _next;

    // _logger = writes errors to the console/log file
    // So we can see what crashed and why
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Try to run the next thing (controller)
            // If it crashes we catch it below
            await _next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Wrong password, invalid token etc
            // HTTP 401 = Unauthorized
            await WriteResponse(context, 401, ex.Message);
        }
        
        catch (KeyNotFoundException ex)
        {
            // User not found, OTP not found etc
            // HTTP 404 = Not Found
            await WriteResponse(context, 404, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            // Email already registered etc
            // HTTP 409 = Conflict
            await WriteResponse(context, 409, ex.Message);
        }
        catch (Exception ex)
        {
            // Anything else we did not expect
            // Log the full error so we can debug it
            // But return a safe message to the user
            // Never expose internal error details to users!
            _logger.LogError(ex, "Unhandled exception");
            await WriteResponse(context, 500,
                "An unexpected error occurred.");
        }
    }

    // Helper method to write the JSON response
    private static async Task WriteResponse(
        HttpContext context,
        int statusCode,
        string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = statusCode;

        // Every error response has the same shape
        // success = false (always for errors)
        // message = what went wrong
        // traceId = unique ID for this request
        //           helps find the error in logs
        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new
            {
                success = false,
                message,
                traceId = context.TraceIdentifier
            }));
    }
}