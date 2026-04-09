using FinVault.IdentityService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinVault.IdentityService.Application.Commands.VerifyRegistrationOtp;

/// <summary>
/// Step 2 of registration:
/// - Loads the PendingRegistration for this email
/// - Verifies the submitted OTP against the stored hash
/// - Marks EmailVerified = true so step 3 (RegisterUser) is allowed
/// </summary>
public class VerifyRegistrationOtpCommandHandler
    : IRequestHandler<VerifyRegistrationOtpCommand, VerifyRegistrationOtpResult>
{
    private readonly IPendingRegistrationRepository _pending;
    private readonly ILogger<VerifyRegistrationOtpCommandHandler> _logger;

    public VerifyRegistrationOtpCommandHandler(
        IPendingRegistrationRepository pending,
        ILogger<VerifyRegistrationOtpCommandHandler> logger)
    {
        _pending = pending;
        _logger  = logger;
    }

    public async Task<VerifyRegistrationOtpResult> Handle(
        VerifyRegistrationOtpCommand cmd,
        CancellationToken ct)
    {
        var email = cmd.Email.ToLowerInvariant();
        _logger.LogInformation("Registration OTP verification for {Email}", email);

        // ── 1. LOAD PENDING REGISTRATION ────────────────────────────────
        var record = await _pending.GetByEmailAsync(email, ct);
        if (record is null)
        {
            _logger.LogWarning("No pending registration found for {Email}", email);
            return new VerifyRegistrationOtpResult(false,
                "No pending registration found. Please request an OTP first.");
        }

        // ── 2. CHECK EXPIRY ─────────────────────────────────────────────
        if (record.ExpiresAt < DateTimeOffset.UtcNow)
        {
            _logger.LogWarning("Registration OTP expired for {Email}", email);
            return new VerifyRegistrationOtpResult(false,
                "OTP has expired. Please request a new registration code.");
        }

        // ── 3. VERIFY HASH ──────────────────────────────────────────────
        if (!BCrypt.Net.BCrypt.Verify(cmd.Code, record.OtpCodeHash))
        {
            _logger.LogWarning("Wrong registration OTP for {Email}", email);
            return new VerifyRegistrationOtpResult(false, "Invalid OTP code.");
        }

        // ── 4. MARK AS VERIFIED ─────────────────────────────────────────
        record.EmailVerified = true;
        await _pending.UpdateAsync(record, ct);

        _logger.LogInformation("Email verified for pending registration {Email}", email);

        return new VerifyRegistrationOtpResult(true,
            "Email verified successfully. You may now complete your registration.");
    }
}
