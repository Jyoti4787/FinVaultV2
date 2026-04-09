using FinVault.CardService.Domain.Interfaces;
using MediatR;

namespace FinVault.CardService.Application.Queries.GetCards;

public class GetCardsQueryHandler
    : IRequestHandler<GetCardsQuery, List<CardSummary>>
{
    private readonly ICreditCardRepository _cards;

    public GetCardsQueryHandler(ICreditCardRepository cards)
        => _cards = cards;

    public async Task<List<CardSummary>> Handle(
        GetCardsQuery query,
        CancellationToken ct)
    {
        var cards = await _cards.GetByUserIdAsync(
            query.UserId, ct);

        return cards.Select(c => {
            var dueDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, c.BillingCycleStartDay).AddDays(20);
            return new CardSummary(
                c.Id,
                c.MaskedNumber,
                c.CardholderName,
                c.Issuer?.Name ?? "Unknown",
                c.Issuer?.Name ?? "Visa", // Using name as network for now
                c.ExpiryMonth,
                c.ExpiryYear,
                c.CreditLimit,
                c.OutstandingBalance,
                c.CreditLimit > 0
                    ? Math.Round(c.OutstandingBalance / c.CreditLimit * 100, 2)
                    : 0,
                c.BillingCycleStartDay,
                dueDate,
                c.IsDefault,
                c.IsVerified
            );
        }).ToList();
    }
}