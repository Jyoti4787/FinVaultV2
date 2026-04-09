// ==================================================================
// FILE : VerifyLoginOtpCommandHandler.cs
// LAYER: Application (Commands)
// WHAT IS THIS?
// Step 2 of the 2-step login.
// User provides OTP from email → we verify it → issue JWT.
// ==================================================================

using FinVault.IdentityService.Domain.Entities;
using FinVault.IdentityService.Domain.Interfaces;
using MediatR;
using RefreshTokenEntity = FinVault.IdentityService.Domain.Entities.RefreshToken;

namespace FinVault.IdentityService.Application.Commands.VerifyLoginOtp;

public class VerifyLoginOtpCommandHandler
    : IRequestHandler<VerifyLoginOtpCommand, VerifyLoginOtpResult>
{
    private readonly IUserRepository        _users;
    private readonly IOTPCodeRepository     _otps;
    private readonly IRefreshTokenRepository _tokens;
    private readonly IJwtTokenService       _jwtService;

    public VerifyLoginOtpCommandHandler(
        IUserRepository         users,
        IOTPCodeRepository      otps,
        IRefreshTokenRepository tokens,
        IJwtTokenService        jwtService)
    {
        _users      = users;
        _otps       = otps;
        _tokens     = tokens;
        _jwtService = jwtService;
    }

    public async Task<VerifyLoginOtpResult> Handle(
        VerifyLoginOtpCommand cmd,
        CancellationToken ct)
    {
        // 1 — Find the user
        var user = await _users.GetByEmailAsync(
            cmd.Email.ToLowerInvariant(), ct)
            ?? throw new UnauthorizedAccessException("User not found.");

        // 2 — Find active Login OTP for this user
        var otp = await _otps.GetActiveCodeAsync(user.Id, "Login", ct);

        // 3 — Validate OTP (not null, not expired, code matches hash)
        if (otp is null ||
            otp.ExpiresAt < DateTimeOffset.UtcNow ||
            !BCrypt.Net.BCrypt.Verify(cmd.OtpCode, otp.CodeHash))
            throw new UnauthorizedAccessException("Invalid or expired OTP.");

        // 4 — Mark OTP as used (one-time only)
        await _otps.MarkAsUsedAsync(otp.Id, ct);

        // 5 — Issue JWT + Refresh Token (same as old step-1 login did)
        var accessToken  = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var expiresAt    = DateTimeOffset.UtcNow.AddMinutes(15);

        await _tokens.AddAsync(new RefreshTokenEntity
        {
            Id        = Guid.NewGuid(),
            UserId    = user.Id,
            Token     = refreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedAt = DateTimeOffset.UtcNow
        }, ct);

        return new VerifyLoginOtpResult(
            accessToken,
            refreshToken,
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            expiresAt);
    }
}
