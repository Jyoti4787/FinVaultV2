using FinVault.CardService.Domain.Interfaces;
using FinVault.Shared.Contracts.Messages;
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

        // If it's a repayment, we MINUS from the debt (OutstandingBalance)
        if (msg.Category == "Credit Card Repayment")
        {
            card.OutstandingBalance = Math.Max(0, card.OutstandingBalance - msg.Amount);
        }
        else
        {
            // If it's a spend (Bill Pay, etc.), we ADD to the debt
            card.OutstandingBalance += msg.Amount;
        }

        card.UpdatedAt = DateTimeOffset.UtcNow;

        await _cards.UpdateAsync(card, context.CancellationToken);
    }
}