using FinVault.IdentityService.Domain.Interfaces;
using MediatR;

namespace FinVault.IdentityService.Application.Commands.VerifyOTP;

// WHAT DOES THIS HANDLER DO?
// Step 1 — find the user
// Step 2 — find their active OTP for the given purpose
// Step 3 — check if code matches and not expired
// Step 4 — mark OTP as used so it cannot be used again

// Published by : MediatR routes here when VerifyOTPCommand sent
// Consumed by  : Nobody — returns true/false to controller
public class VerifyOTPCommandHandler
    : IRequestHandler<VerifyOTPCommand, VerifyOTPResult>
{
    private readonly IUserRepository _users;
    private readonly IOTPCodeRepository _otps;

    public VerifyOTPCommandHandler(
        IUserRepository users,
        IOTPCodeRepository otps)
    {
        _users = users;
        _otps  = otps;
    }

    public async Task<VerifyOTPResult> Handle(
        VerifyOTPCommand cmd,
        CancellationToken ct)
    {
        // Step 1 — Find user
        var user = await _users.GetByEmailAsync(
            cmd.Email.ToLowerInvariant(), ct)
            ?? throw new KeyNotFoundException(
                "User not found.");

        // Step 2 — Find active OTP for this user and purpose
        // Active means not used AND not expired
        var otp = await _otps.GetActiveCodeAsync(
            user.Id, cmd.Purpose, ct);

        // Step 3 — Check three things:
        // a) Does an active OTP even exist?
        // b) Has it expired? (double check)
        // c) Does the code the user typed match the hash?
        // BCrypt.Verify("123456", "$2a$12$abc...") = true/false
        if (otp is null ||
            otp.ExpiresAt < DateTimeOffset.UtcNow ||
            !BCrypt.Net.BCrypt.Verify(cmd.Code, otp.CodeHash))
            // Return false — do NOT throw exception
            // This is not a crash — just wrong code
            return new VerifyOTPResult(
                false, "Invalid or expired OTP.");

        // Step 4 — Mark as used
        // Now this OTP cannot be used again
        // Even if someone intercepts it
        await _otps.MarkAsUsedAsync(otp.Id, ct);

        return new VerifyOTPResult(
            true, "OTP verified successfully.");
    }
}