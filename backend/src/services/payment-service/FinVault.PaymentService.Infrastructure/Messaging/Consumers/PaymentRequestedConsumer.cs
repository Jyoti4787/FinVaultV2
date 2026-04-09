// ==================================================================
// FILE : PaymentRequestedConsumer.cs
// LAYER: Infrastructure (Messaging → Consumers)
// PATH : payment-service/.../Infrastructure/Messaging/Consumers/
//
// WHAT IS THIS?
// This is a RABBITMQ CONSUMER (The "Listener").
// When another service (like a Saga) wants to REQUEST a payment,
// it sends a "PaymentRequestedMessage" to RabbitMQ.
//
// This class catches that message and:
// 1. Sends a "ProcessPaymentCommand" to MediatR
// 2. The command handler then does the actual work
//
// Published by : MassTransit State Machine (Saga)
// Consumed by  : This class — queue: payment-service.PaymentRequested
// ==================================================================

using System.Threading.Tasks;
using FinVault.PaymentService.Application.Commands.ProcessPayment;
using MassTransit;
using MediatR;

namespace FinVault.PaymentService.Infrastructure.Messaging.Consumers;

// IConsumer<T> = MassTransit listener interface
public class PaymentRequestedConsumer 
    : IConsumer<PaymentRequestedMessage>
{
    private readonly IMediator _mediator;

    public PaymentRequestedConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(
        ConsumeContext<PaymentRequestedMessage> context)
    {
        var msg = context.Message;

        // "Delegate" the work to our Application layer handler
        // NOTE: OtpCode is empty here because OTP was already verified
        // at the API controller level before this Saga consumer was triggered.
        // The IPaymentOtpVerifier.VerifyAsync will be skipped for empty OtpCode.
        await _mediator.Send(new ProcessPaymentCommand(
            msg.UserId,
            msg.CardId,
            msg.StatementId,
            msg.Amount,
            msg.Currency,
            msg.CorrelationId,
            OtpCode: string.Empty,  // already verified
            Email:   string.Empty), // already verified
            context.CancellationToken);

    }
}

// ──────────────────────────────────────────────────────────────────
// MESSAGE CONTRACT (Incoming)
// ──────────────────────────────────────────────────────────────────
public record PaymentRequestedMessage(
    Guid UserId,
    Guid CardId,
    Guid StatementId,
    decimal Amount,
    string Currency,
    Guid CorrelationId
);
