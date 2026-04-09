using FinVault.CardService.Domain.Interfaces;
using MassTransit;

namespace FinVault.CardService.Infrastructure.Messaging.Consumers;

// Published by : payment-service via RabbitMQ
// Consumed by  : This class
// What it does : Updates the outstanding balance on the card
//                after a payment is made
public class PaymentCompletedConsumer
    : IConsumer<PaymentCompletedMessage>
{
    private readonly ICreditCardRepository _cards;

    public PaymentCompletedConsumer(ICreditCardRepository cards)
        => _cards = cards;

    public async Task Consume(
        ConsumeContext<PaymentCompletedMessage> context)
    {
        var msg  = context.Message;
        var card = await _cards.GetByIdAsync(
            msg.CardId, context.CancellationToken);

        if (card is null) return;

        // Reduce outstanding balance by payment amount
        // Cannot go below zero
        card.OutstandingBalance = Math.Max(
            0, card.OutstandingBalance - msg.Amount);
        card.UpdatedAt = DateTimeOffset.UtcNow;

        await _cards.UpdateAsync(card, context.CancellationToken);
    }
}

public class PaymentCompletedMessage
{
    public Guid PaymentId { get; set; }
    public Guid UserId    { get; set; }
    public Guid CardId    { get; set; }
    public decimal Amount { get; set; }
    public Guid? CorrelationId { get; set; }
}