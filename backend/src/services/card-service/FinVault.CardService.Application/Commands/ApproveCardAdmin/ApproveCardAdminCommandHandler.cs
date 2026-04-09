using FinVault.CardService.Domain.Interfaces;
using MediatR;

namespace FinVault.CardService.Application.Commands.ApproveCardAdmin;

public class ApproveCardAdminCommandHandler : IRequestHandler<ApproveCardAdminCommand, bool>
{
    private readonly ICreditCardRepository _cards;

    public ApproveCardAdminCommandHandler(ICreditCardRepository cards)
    {
        _cards = cards;
    }

    public async Task<bool> Handle(ApproveCardAdminCommand request, CancellationToken ct)
    {
        var card = await _cards.GetByIdAsync(request.CardId, ct);
        if (card == null) return false;

        card.IsVerified = true;
        card.UpdatedAt = DateTimeOffset.UtcNow;

        await _cards.UpdateAsync(card, ct);

        return true;
    }
}
