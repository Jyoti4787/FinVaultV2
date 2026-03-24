using FinVault.IdentityService.Application.Commands.LoginUser;
using FinVault.IdentityService.Application.Commands.RefreshToken;
using FinVault.IdentityService.Application.Commands.RegisterUser;
using FinVault.IdentityService.Application.Commands.ResetPassword;
using FinVault.IdentityService.Application.Commands.SendOTP;
using FinVault.IdentityService.Application.Commands.VerifyOTP;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinVault.IdentityService.API.Controllers;

// WHAT IS A CONTROLLER?
// A controller is the RECEPTIONIST of your service
// It receives HTTP requests from the outside world
// Hands the work to MediatR
// Gets the result back
// Sends the response
//
// Controllers are THIN — they do NO business logic
// All logic is in Command/Query Handlers
// Controller just says "here MediatR, you deal with this"
//
// [ApiController]    = this is an API controller
//                      enables automatic model validation
// [Route(...)]       = all endpoints start with this URL
// [Produces(...)]    = all responses are JSON

// Receives : HTTP requests from Ocelot Gateway
// Sends to : Angular SPA
[ApiController]
[Route("api/identity/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    // IMediator = MediatR's main interface
    // We send commands and queries through it
    // It finds the right handler automatically
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>Register a new user account</summary>
    // [HttpPost("register")] = responds to
    // POST http://localhost:5001/api/identity/auth/register
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Register(
        // [FromBody] = read the request body as JSON
        // and convert it to RegisterUserCommand object
        [FromBody] RegisterUserCommand command,
        CancellationToken ct)
    {
        // Send command to MediatR
        // MediatR finds RegisterUserCommandHandler
        // Handler runs and returns result
        var result = await _mediator.Send(command, ct);

        // Wrap in standard success response
        return Ok(new { success = true, data = result });
    }

    /// <summary>Login with email and password</summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get new JWT using refresh token</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Send OTP code to email</summary>
    [HttpPost("mfa/send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SendOTP(
        [FromBody] SendOTPCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Verify the OTP code</summary>
    [HttpPost("mfa/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyOTP(
        [FromBody] VerifyOTPCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Reset password using OTP</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(new { success = true, data = result });
    }
}