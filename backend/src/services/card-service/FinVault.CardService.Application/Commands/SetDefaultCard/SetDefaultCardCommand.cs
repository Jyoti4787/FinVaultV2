using MediatR;

namespace FinVault.CardService.Application.Commands.SetDefaultCard;

// Published by : CardsController PATCH /api/cards/{cardId}/default
// Consumed by  : SetDefaultCardCommandHandler
public record SetDefaultCardCommand(
    Guid CardId,
    Guid UserId,
    Guid CorrelationId
) : IRequest<SetDefaultCardResult>;

public record SetDefaultCardResult(string Message);