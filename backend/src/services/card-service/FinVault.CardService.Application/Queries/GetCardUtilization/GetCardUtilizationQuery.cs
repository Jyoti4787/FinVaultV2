using MediatR;

namespace FinVault.CardService.Application.Queries.GetCardUtilization;

// Published by : CardsController GET /api/cards/{cardId}/utilization
// Consumed by  : GetCardUtilizationQueryHandler
public record GetCardUtilizationQuery(
    Guid CardId,
    Guid UserId
) : IRequest<CardUtilizationResult>;

public record CardUtilizationResult(
    Guid CardId,
    string MaskedNumber,
    decimal CreditLimit,
    decimal OutstandingBalance,
    decimal AvailableCredit,
    decimal UtilizationPercent,
    // Alert if utilization is too high
    bool IsHighUtilization);