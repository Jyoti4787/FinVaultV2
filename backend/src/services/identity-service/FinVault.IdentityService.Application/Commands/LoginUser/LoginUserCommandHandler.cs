using FinVault.IdentityService.Domain.Interfaces;
using FinVault.IdentityService.Application.Commands.SendOTP;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinVault.IdentityService.Application.Commands.LoginUser;

// STEP 1 OF 2-STEP LOGIN
// 1. Validate email + password
// 2. Delegate OTP generation to SendOTPCommandHandler (single source of truth)
//    — this ensures rate limiting and old-OTP invalidation apply to login too
// 3. Return { OtpRequired: true } — NO JWT yet
//
// JWT is issued in Step 2: POST /auth/login/verify-otp
public class LoginUserCommandHandler
    : IRequestHandler<LoginUserCommand, LoginUserResult>
{
    private readonly IUserRepository _users;
    private readonly IMediator       _mediator;
    private readonly IRefreshTokenRepository _tokens;
    private readonly IJwtTokenService _jwtService;
    private readonly ILogger<LoginUserCommandHandler> _logger;

    public LoginUserCommandHandler(
        IUserRepository users,
        IMediator mediator,
        IRefreshTokenRepository tokens,
        IJwtTokenService jwtService,
        ILogger<LoginUserCommandHandler> logger)
    {
        _users      = users;
        _mediator   = mediator;
        _tokens     = tokens;
        _jwtService = jwtService;
        _logger     = logger;
    }

    public async Task<LoginUserResult> Handle(
        LoginUserCommand cmd,
        CancellationToken ct)
    {
        _logger.LogInformation("Login attempt for {Email}", cmd.Email);

        // 1 — Validate credentials (same error message for both cases — security best practice)
        var user = await _users.GetByEmailAsync(cmd.Email.ToLowerInvariant(), ct)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(cmd.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed — wrong password for {Email}", cmd.Email);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed — account disabled for {Email}", cmd.Email);
            throw new UnauthorizedAccessException("Account is disabled.");
        }

        // ADMIN EXCEPTION — Admins bypass OTP
        if (user.Role == "Admin")
        {
            _logger.LogInformation("Admin login bypassing OTP for {Email}", cmd.Email);

            var accessToken  = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            await _tokens.AddAsync(new Domain.Entities.RefreshToken
            {
                Id        = Guid.NewGuid(),
                UserId    = user.Id,
                Token     = refreshToken,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                CreatedAt = DateTimeOffset.UtcNow
            }, ct);

            return new LoginUserResult(
                OtpRequired: false,
                Message: "Login successful.",
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                UserId: user.Id,
                Email: user.Email,
                Role: user.Role);
        }

        // 2 — Delegate OTP generation to SendOTPCommandHandler
        //     This is the SINGLE place that generates OTPs.
        //     It handles: rate limiting, invalidating old OTPs, hashing, saving, publishing.
        //     Calling it here means login OTPs go through the same pipeline as all other OTPs.
        await _mediator.Send(new SendOTPCommand(
            user.Email,
            "Login",
            cmd.CorrelationId), ct);

        _logger.LogInformation("Login OTP dispatched for {Email}", cmd.Email);

        return new LoginUserResult(
            OtpRequired: true,
            Message: "OTP sent to your email. Enter it to complete login.");
    }
}
