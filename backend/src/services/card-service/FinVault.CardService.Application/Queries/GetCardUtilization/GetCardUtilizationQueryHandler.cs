using FinVault.CardService.Domain.Interfaces;
using MediatR;

namespace FinVault.CardService.Application.Queries.GetCardUtilization;

public class GetCardUtilizationQueryHandler
    : IRequestHandler<GetCardUtilizationQuery, CardUtilizationResult>
{
    private readonly ICreditCardRepository _cards;

    public GetCardUtilizationQueryHandler(
        ICreditCardRepository cards) => _cards = cards;

    public async Task<CardUtilizationResult> Handle(
        GetCardUtilizationQuery query,
        CancellationToken ct)
    {
        var card = await _cards.GetByIdAsync(query.CardId, ct)
            ?? throw new KeyNotFoundException("Card not found.");

        if (card.UserId != query.UserId)
            throw new UnauthorizedAccessException(
                "You do not own this card.");

        var utilization = card.CreditLimit > 0
            ? Math.Round(
                card.OutstandingBalance / card.CreditLimit * 100, 2)
            : 0;

        return new CardUtilizationResult(
            card.Id,
            card.MaskedNumber,
            card.CreditLimit,
            card.OutstandingBalance,
            card.CreditLimit - card.OutstandingBalance,
            utilization,
            // High utilization = over 80%
            utilization > 80);
    }
}