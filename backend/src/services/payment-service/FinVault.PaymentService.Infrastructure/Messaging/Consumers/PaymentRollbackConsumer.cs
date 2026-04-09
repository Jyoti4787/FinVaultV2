// ==================================================================
// FILE : PaymentRollbackConsumer.cs
// LAYER: Infrastructure (Messaging)
// WHAT IS THIS?
// This is the "UNDO" button.
// If the Saga fails (e.g., Billing was down), this consumer
// receives a message to mark the payment as "Refunded".
// ==================================================================

using FinVault.PaymentService.Domain.Entities;
using FinVault.PaymentService.Domain.Interfaces;
using FinVault.Shared.Contracts.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FinVault.PaymentService.Infrastructure.Messaging.Consumers;

public class PaymentRollbackConsumer : IConsumer<PaymentRollbackRequested>
{
    private readonly IPaymentRepository _repository;
    private readonly ILogger<PaymentRollbackConsumer> _logger;

    public PaymentRollbackConsumer(IPaymentRepository repository, ILogger<PaymentRollbackConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentRollbackRequested> context)
    {
        var msg = context.Message;
        _logger.LogWarning("ROLLBACK: Reversing payment {TransactionId} due to saga failure.", msg.TransactionId);

        var payment = await _repository.GetByIdAsync(msg.TransactionId);
        if (payment != null)
        {
            payment.Status = "Refunded"; // Or add a Refunded status constant
            payment.FailureReason = $"Saga Rollback: {msg.Reason}";
            await _repository.UpdateAsync(payment);
            
            _logger.LogInformation("ROLLBACK SUCCESS: Payment {TransactionId} marked as Refunded.", msg.TransactionId);
        }
    }
}
