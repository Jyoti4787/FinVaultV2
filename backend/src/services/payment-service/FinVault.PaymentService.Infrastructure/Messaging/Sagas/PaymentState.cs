// ==================================================================
// FILE : PaymentState.cs
// LAYER: Infrastructure (Sagas)
// WHAT IS THIS?
// This is the "Journal Entry" for a Saga.
// It stores the current status of a payment process.
// ==================================================================

using MassTransit;
using System;

namespace FinVault.PaymentService.Infrastructure.Messaging.Sagas;

public class PaymentState : SagaStateMachineInstance
{
    // The unique ID for this specific payment story (Correlation ID)
    public Guid CorrelationId { get; set; }

    // Current state (e.g., "Processing", "BillingUpdated", "Completed")
    public string CurrentState { get; set; } = string.Empty;

    // Data we need to remember during the story
    public Guid TransactionId { get; set; }
    public Guid UserId { get; set; }
    public Guid CardId { get; set; }
    public Guid StatementId { get; set; }
    public decimal Amount { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
