using FinVault.IdentityService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinVault.IdentityService.Application.Commands.VerifyOTP;

/// <summary>
/// Verifies a submitted OTP code against the stored hash.
/// Marks the OTP as used on success to prevent replay attacks.
/// </summary>
public class VerifyOTPCommandHandler
    : IRequestHandler<VerifyOTPCommand, VerifyOTPResult>
{
    private readonly IUserRepository    _users;
    private readonly IOTPCodeRepository _otps;
    private readonly ILogger<VerifyOTPCommandHandler> _logger;

    public VerifyOTPCommandHandler(
        IUserRepository users,
        IOTPCodeRepository otps,
        ILogger<VerifyOTPCommandHandler> logger)
    {
        _users  = users;
        _otps   = otps;
        _logger = logger;
    }

    public async Task<VerifyOTPResult> Handle(
        VerifyOTPCommand cmd,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "OTP verification attempt — Email: {Email}, Purpose: {Purpose}",
            cmd.Email, cmd.Purpose);

        // ── 1. FIND USER ──────────────────────────────────────────
        var user = await _users.GetByEmailAsync(cmd.Email.ToLowerInvariant(), ct)
            ?? throw new KeyNotFoundException($"No account found for email '{cmd.Email}'.");

        // ── 2. FIND ACTIVE OTP ────────────────────────────────────
        var otp = await _otps.GetActiveCodeAsync(user.Id, cmd.Purpose, ct);

        // ── 3. VALIDATE ───────────────────────────────────────────
        // Three checks: exists, not expired, code matches hash
        if (otp is null)
        {
            _logger.LogWarning(
                "OTP verification failed — no active OTP found. UserId: {UserId}, Purpose: {Purpose}",
                user.Id, cmd.Purpose);
            return new VerifyOTPResult(false, "No active OTP found. Please request a new one.");
        }

        if (otp.ExpiresAt < DateTimeOffset.UtcNow)
        {
            _logger.LogWarning(
                "OTP verification failed — expired. OtpId: {OtpId}, ExpiredAt: {ExpiresAt}",
                otp.Id, otp.ExpiresAt);
            return new VerifyOTPResult(false, "OTP has expired. Please request a new one.");
        }

        if (!BCrypt.Net.BCrypt.Verify(cmd.Code, otp.CodeHash))
        {
            _logger.LogWarning(
                "OTP verification failed — wrong code. UserId: {UserId}, Purpose: {Purpose}",
                user.Id, cmd.Purpose);
            return new VerifyOTPResult(false, "Invalid OTP code.");
        }

        // ── 4. MARK AS USED ───────────────────────────────────────
        // Prevents replay attacks — same OTP cannot be used twice
        await _otps.MarkAsUsedAsync(otp.Id, ct);

        _logger.LogInformation(
            "OTP verified successfully — OtpId: {OtpId}, UserId: {UserId}, Purpose: {Purpose}",
            otp.Id, user.Id, cmd.Purpose);

        return new VerifyOTPResult(true, "OTP verified successfully.");
    }
}
