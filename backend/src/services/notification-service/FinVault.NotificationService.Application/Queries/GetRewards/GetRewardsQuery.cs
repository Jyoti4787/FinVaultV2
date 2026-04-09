using MediatR;

namespace FinVault.NotificationService.Application.Queries.GetRewards;

public record GetRewardsQuery(Guid UserId) : IRequest<List<RewardDto>>;

public record RewardDto(
    Guid Id,
    Guid UserId,
    int Points,
    string Description,
    DateTime CreatedAt);

public class GetRewardsQueryHandler : IRequestHandler<GetRewardsQuery, List<RewardDto>>
{
    public async Task<List<RewardDto>> Handle(GetRewardsQuery request, CancellationToken ct)
    {
        // Placeholder implementation - replace with actual database query
        await Task.CompletedTask;
        
        return new List<RewardDto>
        {
            new RewardDto(
                Guid.NewGuid(),
                request.UserId,
                1000,
                "Welcome Bonus",
                DateTime.UtcNow.AddDays(-30))
        };
    }
}
