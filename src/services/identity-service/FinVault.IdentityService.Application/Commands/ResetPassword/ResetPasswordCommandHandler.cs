using FinVault.IdentityService.Domain.Interfaces;
using MediatR;

namespace FinVault.IdentityService.Application.Commands.ResetPassword;

// WHAT DOES THIS HANDLER DO?
// Step 1 — find the user
// Step 2 — find their active PasswordReset OTP
// Step 3 — verify the OTP code
// Step 4 — mark OTP as used
// Step 5 — hash new password and save
// Step 6 — revoke ALL existing sessions
//          (logout from all devices for security)

// Published by : MediatR routes here when ResetPasswordCommand sent
// Consumed by  : Nobody — resets password and logs out everywhere
public class ResetPasswordCommandHandler
    : IRequestHandler<ResetPasswordCommand, ResetPasswordResult>
{
    private readonly IUserRepository _users;
    private readonly IOTPCodeRepository _otps;
    private readonly IRefreshTokenRepository _tokens;

    public ResetPasswordCommandHandler(
        IUserRepository users,
        IOTPCodeRepository otps,
        IRefreshTokenRepository tokens)
    {
        _users  = users;
        _otps   = otps;
        _tokens = tokens;
    }

    public async Task<ResetPasswordResult> Handle(
        ResetPasswordCommand cmd,
        CancellationToken ct)
    {
        // Step 1 — Find user
        var user = await _users.GetByEmailAsync(
            cmd.Email.ToLowerInvariant(), ct)
            ?? throw new KeyNotFoundException(
                "User not found.");

        // Step 2 — Find active PasswordReset OTP
        // Purpose MUST be "PasswordReset"
        // A Login OTP cannot be used to reset password
        var otp = await _otps.GetActiveCodeAsync(
            user.Id, "PasswordReset", ct);

        // Step 3 — Verify the OTP
        if (otp is null ||
            otp.ExpiresAt < DateTimeOffset.UtcNow ||
            !BCrypt.Net.BCrypt.Verify(
                cmd.OtpCode, otp.CodeHash))
            throw new UnauthorizedAccessException(
                "Invalid or expired OTP.");

        // Step 4 — Mark OTP as used
        await _otps.MarkAsUsedAsync(otp.Id, ct);

        // Step 5 — Hash new password and save to DB
        // workFactor 12 = slow hash = hard to brute force
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(
            cmd.NewPassword, workFactor: 12);
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _users.UpdateAsync(user, ct);

        // Step 6 — Revoke ALL refresh tokens for this user
        // This logs them out from every device
        // Phone, laptop, tablet — all sessions end
        // This is important security practice
        // After password reset, all old sessions die
        await _tokens.RevokeAllForUserAsync(user.Id, ct);

        return new ResetPasswordResult(
            "Password reset successfully.");
    }
}