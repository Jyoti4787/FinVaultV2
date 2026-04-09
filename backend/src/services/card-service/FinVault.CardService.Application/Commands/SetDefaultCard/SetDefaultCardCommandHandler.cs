using FinVault.CardService.Domain.Interfaces;
using MediatR;

namespace FinVault.CardService.Application.Commands.SetDefaultCard;

public class SetDefaultCardCommandHandler
    : IRequestHandler<SetDefaultCardCommand, SetDefaultCardResult>
{
    private readonly ICreditCardRepository _cards;

    public SetDefaultCardCommandHandler(
        ICreditCardRepository cards) => _cards = cards;

    public async Task<SetDefaultCardResult> Handle(
        SetDefaultCardCommand cmd,
        CancellationToken ct)
    {
        var card = await _cards.GetByIdAsync(cmd.CardId, ct)
            ?? throw new KeyNotFoundException("Card not found.");

        if (card.UserId != cmd.UserId)
            throw new UnauthorizedAccessException(
                "You do not own this card.");

        if (card.IsDeleted)
            throw new InvalidOperationException(
                "Cannot set a deleted card as default.");

        // Step 1 — Clear existing default for this user
        // Only ONE card can be default at a time
        await _cards.ClearDefaultForUserAsync(cmd.UserId, ct);

        // Step 2 — Set this card as default
        card.IsDefault = true;
        card.UpdatedAt = DateTimeOffset.UtcNow;
        await _cards.UpdateAsync(card, ct);

        return new SetDefaultCardResult(
            "Default card updated.");
    }
}