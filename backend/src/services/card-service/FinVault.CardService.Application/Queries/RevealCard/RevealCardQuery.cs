using FinVault.CardService.Domain.Interfaces;
using MediatR;

namespace FinVault.CardService.Application.Queries.RevealCard;

public record RevealCardQuery(Guid CardId, Guid UserId) : IRequest<RevealCardResult>;

public record RevealCardResult(
    string CardNumber,
    string CVV,
    int ExpiryMonth,
    int ExpiryYear,
    string CardholderName,
    string Warning);

public class RevealCardQueryHandler : IRequestHandler<RevealCardQuery, RevealCardResult>
{
    private readonly ICreditCardRepository _cards;

    public RevealCardQueryHandler(ICreditCardRepository cards)
    {
        _cards = cards;
    }

    public async Task<RevealCardResult> Handle(RevealCardQuery request, CancellationToken ct)
    {
        var card = await _cards.GetByIdAsync(request.CardId, ct)
            ?? throw new InvalidOperationException("Card not found");

        if (card.UserId != request.UserId)
            throw new UnauthorizedAccessException("You do not own this card");

        // For now, return masked data - implement actual decryption when encryption service is available
        var maskedNumber = card.MaskedNumber;
        var maskedCvv = "***";

        return new RevealCardResult(
            maskedNumber,
            maskedCvv,
            card.ExpiryMonth,
            card.ExpiryYear,
            card.CardholderName,
            "This sensitive information will only be displayed once. Do not share it with anyone.");
    }
}
