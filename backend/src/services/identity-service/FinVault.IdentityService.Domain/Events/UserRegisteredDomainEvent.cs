using MediatR;

namespace FinVault.IdentityService.Domain.Events;

public record UserRegisteredDomainEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    Guid CorrelationId) : INotification;
