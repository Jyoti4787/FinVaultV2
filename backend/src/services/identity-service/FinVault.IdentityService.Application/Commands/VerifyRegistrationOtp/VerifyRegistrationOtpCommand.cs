using MediatR;

namespace FinVault.IdentityService.Application.Commands.VerifyRegistrationOtp;

/// <summary>
/// Step 2 of registration: user submits the OTP they received.
/// On success, marks the PendingRegistration as EmailVerified = true.
/// </summary>
public record VerifyRegistrationOtpCommand(
    string Email,
    string Code,
    Guid CorrelationId
) : IRequest<VerifyRegistrationOtpResult>;

public record VerifyRegistrationOtpResult(bool IsVerified, string Message);
