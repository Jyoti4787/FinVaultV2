using FinVault.CardService.Domain.Events;
using MassTransit;
using MediatR;

namespace FinVault.CardService.Infrastructure.Messaging.Publishers;

public class CardAddedMessage
{
    public string EventName     { get; set; } = string.Empty;
    public string SourceService { get; set; } = string.Empty;
    public Guid CardId          { get; set; }
    public Guid UserId          { get; set; }
    public string MaskedNumber  { get; set; } = string.Empty;
    public string IssuerName    { get; set; } = string.Empty;
    public Guid CorrelationId   { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}

// Published by : AddCardCommandHandler fires CardAddedDomainEvent
// This class  : MediatR routes here → publishes to RabbitMQ
// Subscribers : notification-service (sends confirmation email)
public class CardAddedPublisher
    : INotificationHandler<CardAddedDomainEvent>
{
    private readonly IPublishEndpoint _bus;
    public CardAddedPublisher(IPublishEndpoint bus) => _bus = bus;

    public async Task Handle(
        CardAddedDomainEvent evt,
        CancellationToken ct)
    {
        await _bus.Publish(new CardAddedMessage
        {
            EventName     = "CardAdded",
            SourceService = "card-service",
            CardId        = evt.CardId,
            UserId        = evt.UserId,
            MaskedNumber  = evt.MaskedNumber,
            IssuerName    = evt.IssuerName,
            CorrelationId = evt.CorrelationId,
            OccurredAt    = DateTimeOffset.UtcNow
        }, ct);
    }
}