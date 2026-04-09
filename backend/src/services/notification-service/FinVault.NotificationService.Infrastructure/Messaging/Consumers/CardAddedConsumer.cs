// ==================================================================
// FILE : CardAddedConsumer.cs
// LAYER: Infrastructure (Messaging → Consumers)
// PATH : notification-service/.../Infrastructure/Messaging/Consumers/
//
// WHAT IS THIS?
// A RabbitMQ listener that waits for "CardAddedMessage".
// When you add a new card in card-service, this class catches the event
// and says: "Hey! Let me create a notification for you".
//
// Published by : card-service
// Consumed by  : This class — queue: notification-service.CardAdded
// ==================================================================

using System.Threading.Tasks;
using FinVault.NotificationService.Application.Commands.SendNotification;
using MassTransit;
using MediatR;

namespace FinVault.NotificationService.Infrastructure.Messaging.Consumers;

public class CardAddedConsumer : IConsumer<CardAddedMessage>
{
    private readonly IMediator _mediator;

    public CardAddedConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<CardAddedMessage> context)
    {
        var msg = context.Message;

        // "Your new card (ENDING IN 1234) has been added successfully!"
        string maskedCard = msg.CardNumber.Length > 4 
            ? msg.CardNumber[^4..] 
            : msg.CardNumber;

        await _mediator.Send(new SendNotificationCommand(
            msg.UserId,
            $"Your new {msg.CardType} card (ending in {maskedCard}) has been added successfully!",
            "Card"
        ));
    }
}

// Contract from card-service
public record CardAddedMessage(
    Guid CardId,
    Guid UserId,
    string CardNumber,
    string CardType
);
