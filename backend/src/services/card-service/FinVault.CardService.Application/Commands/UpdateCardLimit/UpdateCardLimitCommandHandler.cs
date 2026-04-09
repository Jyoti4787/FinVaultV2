using FinVault.CardService.Domain.Interfaces;
using MediatR;

namespace FinVault.CardService.Application.Commands.UpdateCardLimit;

public class UpdateCardLimitCommandHandler
    : IRequestHandler<UpdateCardLimitCommand, UpdateCardLimitResult>
{
    private readonly ICreditCardRepository _cards;

    public UpdateCardLimitCommandHandler(
        ICreditCardRepository cards) => _cards = cards;

    public async Task<UpdateCardLimitResult> Handle(
        UpdateCardLimitCommand cmd,
        CancellationToken ct)
    {
        var card = await _cards.GetByIdAsync(cmd.CardId, ct)
            ?? throw new KeyNotFoundException("Card not found.");

        if (card.UserId != cmd.UserId)
            throw new UnauthorizedAccessException(
                "You do not own this card.");

        if (cmd.NewLimit <= 0)
            throw new InvalidOperationException(
                "Credit limit must be greater than zero.");

        card.CreditLimit = cmd.NewLimit;
        card.UpdatedAt   = DateTimeOffset.UtcNow;
        await _cards.UpdateAsync(card, ct);

        return new UpdateCardLimitResult(
            "Credit limit updated.");
    }
}