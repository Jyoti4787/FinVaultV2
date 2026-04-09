using MediatR;

namespace FinVault.CardService.Application.Commands.RemoveCard;

// Published by : CardsController DELETE /api/cards/{cardId}
// Consumed by  : RemoveCardCommandHandler
public record RemoveCardCommand(
    Guid CardId,
    Guid UserId,
    Guid CorrelationId
) : IRequest<RemoveCardResult>;

public record RemoveCardResult(string Message);