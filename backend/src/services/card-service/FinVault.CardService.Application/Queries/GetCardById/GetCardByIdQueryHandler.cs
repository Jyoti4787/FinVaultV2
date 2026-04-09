using FinVault.CardService.Domain.Interfaces;
using MediatR;

namespace FinVault.CardService.Application.Queries.GetCardById;

public class GetCardByIdQueryHandler
    : IRequestHandler<GetCardByIdQuery, CardDetail>
{
    private readonly ICreditCardRepository _cards;

    public GetCardByIdQueryHandler(ICreditCardRepository cards)
        => _cards = cards;

    public async Task<CardDetail> Handle(
        GetCardByIdQuery query,
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

        return new CardDetail(
            card.Id,
            card.MaskedNumber,
            card.CardholderName,
            card.Issuer?.Name ?? "Unknown",
            card.ExpiryMonth,
            card.ExpiryYear,
            card.CreditLimit,
            card.OutstandingBalance,
            card.CreditLimit - card.OutstandingBalance,
            utilization,
            card.BillingCycleStartDay,
            card.IsDefault,
            card.IsVerified,
            card.CreatedAt);
    }
}