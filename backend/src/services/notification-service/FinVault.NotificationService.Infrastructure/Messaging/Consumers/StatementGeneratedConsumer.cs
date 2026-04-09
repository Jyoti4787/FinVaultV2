// ==================================================================
// FILE : StatementGeneratedConsumer.cs
// LAYER: Infrastructure (Messaging → Consumers)
// PATH : notification-service/.../Infrastructure/Messaging/Consumers/
// ==================================================================

using System.Threading.Tasks;
using FinVault.NotificationService.Application.Commands.SendNotification;
using MassTransit;
using MediatR;

namespace FinVault.NotificationService.Infrastructure.Messaging.Consumers;

public class StatementGeneratedConsumer : IConsumer<StatementGeneratedMessage>
{
    private readonly IMediator _mediator;

    public StatementGeneratedConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<StatementGeneratedMessage> context)
    {
        var msg = context.Message;

        await _mediator.Send(new SendNotificationCommand(
            msg.UserId,
            $"Your monthly statement for card ending in {msg.CardId.ToString()[^4..]} is now ready! Amount due: ₹{msg.TotalAmount:N2}.",
            "Billing"
        ));
    }
}

// Contract from billing-service
public record StatementGeneratedMessage(
    Guid StatementId,
    Guid UserId,
    Guid CardId,
    decimal TotalAmount,
    DateTime DueDate
);
