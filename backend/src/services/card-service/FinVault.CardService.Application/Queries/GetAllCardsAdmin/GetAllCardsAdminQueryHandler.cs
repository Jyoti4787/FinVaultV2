using FinVault.CardService.Application.Queries.GetCards;
using FinVault.CardService.Domain.Interfaces;
using MediatR;

namespace FinVault.CardService.Application.Queries.GetAllCardsAdmin;

public class GetAllCardsAdminQueryHandler : IRequestHandler<GetAllCardsAdminQuery, List<CardSummary>>
{
    private readonly ICreditCardRepository _cards;

    public GetAllCardsAdminQueryHandler(ICreditCardRepository cards)
    {
        _cards = cards;
    }

    public async Task<List<CardSummary>> Handle(GetAllCardsAdminQuery request, CancellationToken ct)
    {
        var allCards = await _cards.GetAllAsync(ct);

        return allCards.Select(c => {
            var dueDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, c.BillingCycleStartDay).AddDays(20);
            return new CardSummary(
                c.Id,
                c.MaskedNumber,
                c.CardholderName,
                c.Issuer?.Name ?? "Unknown",
                c.Issuer?.Name ?? "Visa",
                c.ExpiryMonth,
                c.ExpiryYear,
                c.CreditLimit,
                c.OutstandingBalance,
                c.CreditLimit > 0 ? Math.Round(c.OutstandingBalance / c.CreditLimit * 100, 2) : 0,
                c.BillingCycleStartDay,
                dueDate,
                c.IsDefault,
                c.IsVerified
            );
        }).ToList();
    }
}
