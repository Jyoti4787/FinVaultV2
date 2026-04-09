using FinVault.IdentityService.Application.Commands.CompleteRegistration;
using FinVault.IdentityService.Application.Commands.InitiateRegistration;
using FinVault.IdentityService.Application.Commands.LoginUser;
using FinVault.IdentityService.Application.Commands.RefreshToken;
using FinVault.IdentityService.Application.Commands.RegisterUser;
using FinVault.IdentityService.Application.Commands.ResetPassword;
using FinVault.IdentityService.Application.Commands.SendOTP;
using FinVault.IdentityService.Application.Commands.SendRegistrationOtp;
using FinVault.IdentityService.Application.Commands.VerifyLoginOtp;
using FinVault.IdentityService.Application.Commands.VerifyOTP;
using FinVault.IdentityService.Application.Commands.VerifyRegistrationOtp;
using FinVault.IdentityService.Domain.Entities;
using FinVault.IdentityService.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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

    // ──── REGISTRATION FLOW (2 steps) ────────────────────────────────────
    // Step 1 → POST /register        → stores user data + sends OTP to email (no account created yet)
    // Step 2 → POST /register/verify → verifies OTP + creates the actual user account

    /// <summary>Step 1 — Register user and send OTP to email for verification</summary>
    [HttpPost("register")]
    [EnableRateLimiting("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken ct)
    {
        var cmd = new InitiateRegistrationCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            Guid.NewGuid());
        var result = await _mediator.Send(cmd, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Step 2 — Verify OTP and complete registration</summary>
    [HttpPost("register/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyRegistration(
        [FromBody] VerifyRegistrationRequest request,
        CancellationToken ct)
    {
        var cmd = new CompleteRegistrationCommand(request.Email, request.Code, Guid.NewGuid());
        var result = await _mediator.Send(cmd, ct);
        if (!result.Success)
            return BadRequest(new { success = false, message = result.Message });
        return Ok(new { success = true, data = result });
    }

    /// <summary>Login with email and password — sends OTP to email</summary>
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        
        // TEMPORARY: If OTP is disabled, generate JWT directly
        if (!result.OtpRequired && result.UserId.HasValue)
        {
            var jwtService = HttpContext.RequestServices.GetRequiredService<IJwtTokenService>();
            var userRepo = HttpContext.RequestServices.GetRequiredService<IUserRepository>();
            
            var user = await userRepo.GetByIdAsync(result.UserId.Value, ct);
            if (user != null)
            {
                var accessToken = jwtService.GenerateAccessToken(user);
                var refreshToken = jwtService.GenerateRefreshToken();
                
                // Save refresh token
                var refreshTokenRepo = HttpContext.RequestServices.GetRequiredService<IRefreshTokenRepository>();
                await refreshTokenRepo.AddAsync(new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                    CreatedAt = DateTimeOffset.UtcNow
                }, ct);
                
                return Ok(new { 
                    success = true, 
                    data = new {
                        message = result.Message,
                        userId = user.Id,
                        email = user.Email,
                        role = user.Role,
                        accessToken = accessToken,
                        refreshToken = refreshToken
                    }
                });
            }
        }
        
        return Ok(new { success = true, data = result });
    }

    /// <summary>Step 2 of login — verify OTP and receive JWT token</summary>
    [HttpPost("login/verify-otp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyLoginOtp(
        [FromBody] VerifyLoginOtpCommand command,
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
    [EnableRateLimiting("otp")]
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

    /// <summary>Verify email address using token</summary>
    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyEmail(
        [FromBody] VerifyEmailRequest request,
        CancellationToken ct)
    {
        // Use the existing OTP verification mechanism
        var command = new VerifyOTPCommand(request.Email, request.Token, "EmailVerification", Guid.NewGuid());
        var result = await _mediator.Send(command, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Send forgot password email with reset link</summary>
    [HttpPost("forgot-password")]
    [EnableRateLimiting("otp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct)
    {
        // Use the existing OTP mechanism for password reset
        var command = new SendOTPCommand(request.Email, "PasswordReset", Guid.NewGuid());
        var result = await _mediator.Send(command, ct);
        return Ok(new { success = true, message = "Password reset code sent to your email." });
    }
}

public record VerifyEmailRequest(string Email, string Token);
public record ForgotPasswordRequest(string Email);
public record RegisterUserRequest(string Email, string Password, string FirstName, string LastName);
public record VerifyRegistrationRequest(string Email, string Code);





