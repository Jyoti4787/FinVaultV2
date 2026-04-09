using FinVault.CardService.Domain.Events;
using MassTransit;
using MediatR;

namespace FinVault.CardService.Infrastructure.Messaging.Publishers;

public class CardExpirySoonMessage
{
    public string EventName    { get; set; } = string.Empty;
    public string SourceService { get; set; } = string.Empty;
    public Guid CardId         { get; set; }
    public Guid UserId         { get; set; }
    public string MaskedNumber { get; set; } = string.Empty;
    public int ExpiryMonth     { get; set; }
    public int ExpiryYear      { get; set; }
    public Guid CorrelationId  { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}

public class CardExpirySoonPublisher
    : INotificationHandler<CardExpirySoonDomainEvent>
{
    private readonly IPublishEndpoint _bus;
    public CardExpirySoonPublisher(IPublishEndpoint bus)
        => _bus = bus;

    public async Task Handle(
        CardExpirySoonDomainEvent evt,
        CancellationToken ct)
    {
        await _bus.Publish(new CardExpirySoonMessage
        {
            EventName     = "CardExpirySoon",
            SourceService = "card-service",
            CardId        = evt.CardId,
            UserId        = evt.UserId,
            MaskedNumber  = evt.MaskedNumber,
            ExpiryMonth   = evt.ExpiryMonth,
            ExpiryYear    = evt.ExpiryYear,
            CorrelationId = evt.CorrelationId,
            OccurredAt    = DateTimeOffset.UtcNow
        }, ct);
    }
}