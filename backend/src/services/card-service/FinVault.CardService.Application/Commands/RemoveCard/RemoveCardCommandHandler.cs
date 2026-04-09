using FinVault.CardService.Domain.Interfaces;
using MediatR;

namespace FinVault.CardService.Application.Commands.RemoveCard;

// Soft deletes a card — sets IsDeleted = true
// Never hard deletes — payment history still references it
public class RemoveCardCommandHandler
    : IRequestHandler<RemoveCardCommand, RemoveCardResult>
{
    private readonly ICreditCardRepository _cards;

    public RemoveCardCommandHandler(ICreditCardRepository cards)
        => _cards = cards;

    public async Task<RemoveCardResult> Handle(
        RemoveCardCommand cmd,
        CancellationToken ct)
    {
        var card = await _cards.GetByIdAsync(cmd.CardId, ct)
            ?? throw new KeyNotFoundException("Card not found.");

        // Make sure user owns this card
        // Prevents user A from deleting user B's card
        if (card.UserId != cmd.UserId)
            throw new UnauthorizedAccessException(
                "You do not own this card.");

        if (card.IsDeleted)
            throw new InvalidOperationException(
                "Card is already removed.");

        // Soft delete — just mark as deleted
        card.IsDeleted  = true;
        card.DeletedAt  = DateTimeOffset.UtcNow;
        card.UpdatedAt  = DateTimeOffset.UtcNow;

        await _cards.UpdateAsync(card, ct);

        return new RemoveCardResult(
            "Card removed successfully.");
    }
}