using MediatR;

namespace FinVault.NotificationService.Application.Commands.RedeemReward;

public record RedeemRewardCommand(
    Guid UserId,
    int Points,
    string Reason) : IRequest<RedeemRewardResult>;

public record RedeemRewardResult(
    Guid RedemptionId,
    int PointsRedeemed,
    string Status);

public class RedeemRewardCommandHandler 
    : IRequestHandler<RedeemRewardCommand, RedeemRewardResult>
{
    public async Task<RedeemRewardResult> Handle(
        RedeemRewardCommand request, 
        CancellationToken ct)
    {
        // Placeholder implementation - replace with actual business logic
        await Task.CompletedTask;

        return new RedeemRewardResult(
            Guid.NewGuid(),
            request.Points,
            "Redeemed");
    }
}
