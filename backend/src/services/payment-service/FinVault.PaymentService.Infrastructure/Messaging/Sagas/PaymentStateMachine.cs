// ==================================================================
// FILE : PaymentStateMachine.cs
// LAYER: Infrastructure (Sagas)
// WHAT IS THIS?
// The "Conductor" of the Payment Saga.
// It defines the rules: "If this happens, do that next".
// ==================================================================

using FinVault.Shared.Contracts.Messages;
using MassTransit;
using System;
namespace FinVault.PaymentService.Infrastructure.Messaging.Sagas;

public class PaymentStateMachine : MassTransitStateMachine<PaymentState>
{
    // 1. Define the States (The Chapters of our Story)
    public State Processing { get; private set; } = null!;
    public State BillingUpdating { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    // 2. Define the Events (What triggers a change?)
    public Event<ProcessPaymentSagaRequested> PaymentRequested { get; private set; } = null!;
    public Event<BillingStatementUpdated> BillingConfirmed { get; private set; } = null!;
    public Event<Fault<ProcessPaymentSagaRequested>> PaymentFailed { get; private set; } = null!;

    public PaymentStateMachine()
    {
        // Tell MassTransit where the state is stored
        InstanceState(x => x.CurrentState);

        // Tell MassTransit how to find the right Saga instance
        Event(() => PaymentRequested, x => x.CorrelateById(m => m.Message.TransactionId));
        Event(() => BillingConfirmed, x => x.CorrelateById(m => m.Message.TransactionId));
        // Fault events correlate via the original message's TransactionId
        Event(() => PaymentFailed, x => x.CorrelateById(m => m.Message.Message.TransactionId));

        // ── THE STORY FLOW (SAGA DEFINITION) ──────────────────────────

        Initially(
            When(PaymentRequested)
                .Then(context => {
                    context.Saga.TransactionId = context.Message.TransactionId;
                    context.Saga.Amount = context.Message.Amount;
                    context.Saga.UserId = context.Message.UserId;
                    context.Saga.StatementId = context.Message.StatementId;
                    context.Saga.CreatedAt = DateTime.UtcNow;
                })
                .TransitionTo(Processing)
                // In a real app, you'd send a command to actually deduct money here
                .Publish(context => new PaymentSettled(context.Saga.TransactionId, context.Saga.StatementId, context.Saga.Amount))
        );

        During(Processing,
            When(BillingConfirmed)
                .TransitionTo(Completed)
                .Then(context => context.Saga.UpdatedAt = DateTime.UtcNow)
                // Notify the user that their payment is fully complete
                .Publish(context => new PaymentCompletedMessage(
                    context.Saga.TransactionId,
                    context.Saga.UserId,
                    context.Saga.CardId,
                    context.Saga.Amount,
                    context.Saga.TransactionId))
                .Finalize()
        );

        // ── ERROR HANDLING (THE ROLLBACK) ──────────────────────────────────
        DuringAny(
            When(PaymentFailed)
                .Then(context => {
                    // Log the failure
                })
                .TransitionTo(Failed)
                // Shouts to the system that we need to undo things!
                .Publish(context => new PaymentRollbackRequested(context.Saga.TransactionId, context.Saga.Amount, "System Hub Failure"))
        );
    }
}
