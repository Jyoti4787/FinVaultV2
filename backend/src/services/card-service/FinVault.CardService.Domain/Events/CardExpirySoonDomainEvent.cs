using MediatR;

namespace FinVault.CardService.Domain.Events;

// Published by : Background job (checks cards expiring in 30 days)
// Consumed by  : CardExpirySoonPublisher → RabbitMQ
// Then heard by: notification-service (sends expiry reminder email)
public record CardExpirySoonDomainEvent(
    Guid CardId,
    Guid UserId,
    string MaskedNumber,
    int ExpiryMonth,
    int ExpiryYear,
    Guid CorrelationId) : INotification;