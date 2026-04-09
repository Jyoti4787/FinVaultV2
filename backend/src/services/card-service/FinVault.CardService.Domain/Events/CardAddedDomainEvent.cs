using MediatR;

namespace FinVault.CardService.Domain.Events;

// Published by : AddCardCommandHandler after card saved
// Consumed by  : CardAddedPublisher → RabbitMQ
// Then heard by: notification-service (sends confirmation email)
public record CardAddedDomainEvent(
    Guid CardId,
    Guid UserId,
    string MaskedNumber,
    string IssuerName,
    Guid CorrelationId) : INotification;