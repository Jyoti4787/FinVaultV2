using FinVault.NotificationService.Application.Commands.RedeemReward;
using FinVault.NotificationService.Application.Queries.GetRewards;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinVault.NotificationService.API.Controllers;

[ApiController]
[Route("api/rewards")]
[Produces("application/json")]
[Authorize]
public class RewardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RewardsController(IMediator mediator) => _mediator = mediator;

    private Guid GetUserId() => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get user rewards</summary>
    [HttpGet]
    public async Task<IActionResult> GetRewards(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRewardsQuery(GetUserId()), ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Redeem reward points</summary>
    [HttpPost("redeem")]
    public async Task<IActionResult> RedeemPoints([FromBody] RedeemRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new RedeemRewardCommand(GetUserId(), request.Points, request.Reason), ct);
        return Ok(new { success = true, data = result });
    }
}

public record RedeemRequest(int Points, string Reason);
