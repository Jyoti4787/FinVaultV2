using MediatR;

namespace FinVault.CardService.Application.Commands.UpdateCardLimit;

// Published by : CardsController PATCH /api/cards/{cardId}/limit
// Consumed by  : UpdateCardLimitCommandHandler
public record UpdateCardLimitCommand(
    Guid CardId,
    Guid UserId,
    decimal NewLimit,
    Guid CorrelationId
) : IRequest<UpdateCardLimitResult>;

public record UpdateCardLimitResult(string Message);