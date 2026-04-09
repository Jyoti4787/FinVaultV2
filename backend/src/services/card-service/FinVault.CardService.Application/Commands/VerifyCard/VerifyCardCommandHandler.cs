using FinVault.CardService.Domain.Interfaces;
using MediatR;

namespace FinVault.CardService.Application.Commands.VerifyCard;

public class VerifyCardCommandHandler
    : IRequestHandler<VerifyCardCommand, VerifyCardResult>
{
    private readonly ICreditCardRepository _cards;

    public VerifyCardCommandHandler(
        ICreditCardRepository cards) => _cards = cards;

    public async Task<VerifyCardResult> Handle(
        VerifyCardCommand cmd,
        CancellationToken ct)
    {
        var card = await _cards.GetByIdAsync(cmd.CardId, ct)
            ?? throw new KeyNotFoundException("Card not found.");

        if (card.UserId != cmd.UserId)
            throw new UnauthorizedAccessException(
                "You do not own this card.");

        if (card.IsVerified)
            throw new InvalidOperationException(
                "Card is already verified.");

        // In demo — just mark as verified
        // In production — Rs.1 micro-auth with payment gateway
        card.IsVerified = true;
        card.UpdatedAt  = DateTimeOffset.UtcNow;
        await _cards.UpdateAsync(card, ct);

        return new VerifyCardResult(
            "Card verified successfully.");
    }
}