using MediatR;

namespace FinVault.IdentityService.Application.Commands.InitiateRegistration;

/// <summary>
/// Step 1 of registration: Store user data and send OTP to email.
/// No user account is created yet — data is stored in PendingRegistration.
/// </summary>
public record InitiateRegistrationCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    Guid CorrelationId
) : IRequest<InitiateRegistrationResult>;

public record InitiateRegistrationResult(string Message);
