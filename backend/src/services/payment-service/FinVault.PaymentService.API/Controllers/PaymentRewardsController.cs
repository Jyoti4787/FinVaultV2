using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using FinVault.Shared.Contracts.Messages;

namespace FinVault.PaymentService.API.Controllers;

[ApiController]
[Route("api/payment-rewards")]
[Produces("application/json")]
[Authorize]
public class PaymentRewardsController : ControllerBase
{
    private readonly FinVault.PaymentService.Infrastructure.Persistence.PaymentDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public PaymentRewardsController(
        FinVault.PaymentService.Infrastructure.Persistence.PaymentDbContext dbContext,
        IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    private Guid GetUserId() => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get reward points summary and history for the user</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRewards()
    {
        var userId = GetUserId();

        var rewards = await _dbContext.RewardPoints
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        int totalPoints = rewards.Sum(r => r.Type == "Earned" ? r.Points : -r.Points);
        totalPoints = Math.Max(0, totalPoints);

        var history = rewards.Select(r => new
        {
            r.Id,
            r.Points,
            r.Description,
            r.Type,
            r.CreatedAt
        });

        return Ok(new
        {
            success = true,
            data = new
            {
                Points = totalPoints,
                CashbackValue = Math.Round(totalPoints * 0.10m, 2), // ₹0.10 per point
                History = history
            }
        });
    }

    /// <summary>Redeem reward points as cashback payment discount</summary>
    [HttpPost("redeem")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RedeemPoints([FromBody] RedeemPointsRequest request)
    {
        var userId = GetUserId();

        var rewards = await _dbContext.RewardPoints
            .Where(r => r.UserId == userId)
            .ToListAsync();

        int totalPoints = rewards.Sum(r => r.Type == "Earned" ? r.Points : -r.Points);
        totalPoints = Math.Max(0, totalPoints);

        if (request.Points < 100)
            return BadRequest(new { success = false, message = "Minimum 100 points required to redeem." });

        if (request.Points > totalPoints)
            return BadRequest(new { success = false, message = $"Insufficient points. You have {totalPoints} points available." });

        decimal cashback = Math.Round(request.Points * 0.10m, 2);

        var redemption = new FinVault.PaymentService.Domain.Entities.RewardPoint
        {
            UserId = userId,
            PaymentId = Guid.Empty,
            CardId = request.CardId,
            Points = request.Points,
            Description = $"Redeemed for cashback — ₹{cashback} to card {request.CardId.ToString().Substring(0,8)}...",
            Type = "Redeemed"
        };

        _dbContext.RewardPoints.Add(redemption);
        await _dbContext.SaveChangesAsync();

        // Publish message so Card Service can update the card balance
        await _publishEndpoint.Publish<IRewardRedeemedMessage>(new
        {
            UserId = userId,
            CardId = request.CardId,
            Amount = cashback,
            RedeemedAt = DateTime.UtcNow
        });

        return Ok(new
        {
            success = true,
            data = new
            {
                PointsRedeemed = request.Points,
                CashbackValue = cashback,
                RemainingPoints = totalPoints - request.Points,
                Message = $"Successfully redeemed {request.Points} points for ₹{cashback} cashback! Your card balance will be updated shortly 🎉"
            }
        });
    }
}

public record RedeemPointsRequest(int Points, Guid CardId);
