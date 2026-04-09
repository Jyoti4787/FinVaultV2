using MediatR;

namespace FinVault.CardService.Application.Queries.GetCardById;

// Published by : CardsController GET /api/cards/{cardId}
// Consumed by  : GetCardByIdQueryHandler
public record GetCardByIdQuery(
    Guid CardId,
    Guid UserId
) : IRequest<CardDetail>;

// Full detail — used in detail view
public record CardDetail(
    Guid CardId,
    string MaskedNumber,
    string CardholderName,
    string IssuerName,
    int ExpiryMonth,
    int ExpiryYear,
    decimal CreditLimit,
    decimal OutstandingBalance,
    decimal AvailableCredit,
    decimal UtilizationPercent,
    int BillingCycleStartDay,
    bool IsDefault,
    bool IsVerified,
    DateTimeOffset CreatedAt);