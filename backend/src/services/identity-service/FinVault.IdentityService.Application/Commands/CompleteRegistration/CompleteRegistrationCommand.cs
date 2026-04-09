using MediatR;

namespace FinVault.IdentityService.Application.Commands.CompleteRegistration;

/// <summary>
/// Step 2 of registration: Verify OTP and create the actual user account.
/// </summary>
public record CompleteRegistrationCommand(
    string Email,
    string Code,
    Guid CorrelationId
) : IRequest<CompleteRegistrationResult>;

public record CompleteRegistrationResult(
    bool Success,
    string Message,
    Guid? UserId = null,
    string? Email = null);
