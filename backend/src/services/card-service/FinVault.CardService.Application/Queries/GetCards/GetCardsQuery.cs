using MediatR;

namespace FinVault.CardService.Application.Queries.GetCards;

// Published by : CardsController GET /api/cards
// Consumed by  : GetCardsQueryHandler
public record GetCardsQuery(
    Guid UserId
) : IRequest<List<CardSummary>>;

// Lightweight summary — used in list view
public record CardSummary(
    Guid CardId,
    string MaskedNumber,
    string CardholderName,
    string IssuerName,
    string Network,
    int ExpiryMonth,
    int ExpiryYear,
    decimal CreditLimit,
    decimal OutstandingBalance,
    decimal UtilizationPercent,
    int BillingCycleStartDay,
    DateTime? DueDate,
    bool IsDefault,
    bool IsVerified);