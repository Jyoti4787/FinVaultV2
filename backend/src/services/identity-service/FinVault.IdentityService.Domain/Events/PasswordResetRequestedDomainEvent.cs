using MediatR;

namespace FinVault.IdentityService.Domain.Events;

public record PasswordResetRequestedDomainEvent(
    Guid UserId,
    string Email,
    string OtpCode,
    Guid CorrelationId) : INotification;
