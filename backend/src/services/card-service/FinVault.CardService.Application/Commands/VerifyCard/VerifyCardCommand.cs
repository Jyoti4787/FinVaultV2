using MediatR;

namespace FinVault.CardService.Application.Commands.VerifyCard;

// Published by : CardsController POST /api/cards/{cardId}/verify
// Consumed by  : VerifyCardCommandHandler
// In real life: Rs.1 is charged and refunded to verify ownership
// In demo:     We just mark it as verified
public record VerifyCardCommand(
    Guid CardId,
    Guid UserId,
    Guid CorrelationId
) : IRequest<VerifyCardResult>;

public record VerifyCardResult(string Message);