// ==================================================================
// FILE : PaymentSagaEvents.cs
// LAYER: Shared / Contracts
// ==================================================================

using System;

namespace FinVault.Shared.Contracts.Messages;

// Phase 1: Payment is requested (Saga Starts)
public record ProcessPaymentSagaRequested(
    Guid TransactionId,
    Guid CardId,
    Guid UserId,
    Guid StatementId,
    decimal Amount,
    string Description);

// Phase 2: Payment is successful
public record PaymentSettled(Guid TransactionId, Guid StatementId, decimal Amount);

// Phase 3: Billing service confirms update
public record BillingStatementUpdated(Guid TransactionId, Guid StatementId);

// Phase 4: Something went wrong (Compensation/Rollback)
public record PaymentRollbackRequested(Guid TransactionId, decimal Amount, string Reason);

// Phase 5: Payment fully completed — sent to Notification Service
public record PaymentCompletedMessage(
    Guid PaymentId,
    Guid UserId,
    Guid CardId,
    decimal Amount,
    Guid? CorrelationId,
    string? UserEmail = null);   // Optional — for sending email receipt

