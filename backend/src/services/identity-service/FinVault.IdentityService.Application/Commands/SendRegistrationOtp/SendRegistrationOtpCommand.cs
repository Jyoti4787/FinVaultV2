using MediatR;

namespace FinVault.IdentityService.Application.Commands.SendRegistrationOtp;

/// <summary>
/// Step 1 of registration: send OTP to email BEFORE creating any user account.
/// The email is stored as a PendingRegistration with an OTP hash.
/// </summary>
public record SendRegistrationOtpCommand(
    string Email,
    Guid CorrelationId
) : IRequest<SendRegistrationOtpResult>;

public record SendRegistrationOtpResult(string Message);
