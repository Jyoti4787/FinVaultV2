// ==================================================================
// FILE : ProcessPaymentCommand.cs
// LAYER: Application (Commands)
// PATH : payment-service/FinVault.PaymentService.Application/Commands/ProcessPayment/
//
// WHAT IS THIS?
// This is a "COMMAND" — a request to DO something.
// In this case: "Please process a credit card payment of ₹X".
//
// It's like a FORM you fill and submit.
// Once submitted, MediatR finds the "Handler" who knows how 
// to process this form.
//
// Published by : PaymentsController POST /api/payments
// Consumed by  : ProcessPaymentCommandHandler
// ==================================================================

using System;
using MediatR;

namespace FinVault.PaymentService.Application.Commands.ProcessPayment;

// IRequest<ProcessPaymentResult> means "after you process this, 
// give me back a result object"
public record ProcessPaymentCommand(
    Guid UserId,       // who is paying?
    Guid CardId,       // which card to charge?
    Guid StatementId,  // which bill are they paying?
    decimal Amount,    // how much?
    string Currency,   // e.g. INR
    Guid CorrelationId, // tracking ID for saga
    string OtpCode,    // 6-digit OTP verified before payment executes
    string Email       // user's email — needed to verify OTP with identity-service
) : IRequest<ProcessPaymentResult>;



// This is what the handler returns to the controller
public record ProcessPaymentResult(
    Guid PaymentId,
    string Status,     // Success / Failed
    string? TransactionId,
    string Message
);
