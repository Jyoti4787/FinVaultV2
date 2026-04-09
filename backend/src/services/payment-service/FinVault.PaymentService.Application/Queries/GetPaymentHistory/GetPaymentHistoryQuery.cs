// ==================================================================
// FILE : GetPaymentHistoryQuery.cs
// LAYER: Application (Queries)
// PATH : payment-service/FinVault.PaymentService.Application/Queries/GetPaymentHistory/
//
// WHAT IS THIS?
// This is a "QUERY" — a request to READ data (no changes allowed!).
// "Show me all the payments I've ever made."
//
// Published by : PaymentsController GET /api/payments/history
// Consumed by  : GetPaymentHistoryQueryHandler
// ==================================================================

using System;
using System.Collections.Generic;
using MediatR;

namespace FinVault.PaymentService.Application.Queries.GetPaymentHistory;

public record GetPaymentHistoryQuery(
    Guid UserId // whose history?
) : IRequest<IEnumerable<PaymentHistoryDto>>;

// A "DTO" (Data Transfer Object) is a simplified version of our 
// Payment entity, safe to send to the Angular app
public record PaymentHistoryDto(
    Guid PaymentId,
    Guid CardId,
    decimal Amount,
    string Status,
    DateTime Date,
    string? TransactionId
);
