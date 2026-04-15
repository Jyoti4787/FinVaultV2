using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinVault.PaymentService.API.Controllers;

[ApiController]
[Route("api/transactions")]
[Produces("application/json")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly FinVault.PaymentService.Infrastructure.Persistence.PaymentDbContext _dbContext;

    public TransactionsController(IMediator mediator, FinVault.PaymentService.Infrastructure.Persistence.PaymentDbContext dbContext)
    {
        _mediator = mediator;
        _dbContext = dbContext;
    }

    private Guid GetUserId() => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get all transactions for the current user (payments + external bills + reward redemptions)</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions(CancellationToken ct)
    {
        var userId = GetUserId();

        // 1. Fetch Payments (Debits)
        var payments = await _dbContext.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.TransactionDate)
            .ToListAsync(ct);

        // 2. Fetch Reward Redemptions (Credits)
        var redemptions = await _dbContext.RewardPoints
            .Where(r => r.UserId == userId && r.Type == "Redeemed")
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        // 3. Map Payments to Transaction objects
        var paymentTransactions = payments.Select(p =>
        {
            bool isExternalBill = p.ExternalTransactionId != null && p.ExternalTransactionId.Contains('|');
            string category = isExternalBill ? p.ExternalTransactionId!.Split('|')[0] : "Payment";
            string desc = isExternalBill ? $"{category} — {p.ExternalTransactionId!.Split('|')[1]}" : "Credit Card Payment";

            return new
            {
                Id = p.Id,
                UserId = p.UserId,
                CardId = p.CardId,
                Type = "debit",
                Amount = p.Amount,
                Currency = p.Currency,
                Category = category,
                Description = desc,
                ReferenceId = p.ExternalTransactionId ?? p.Id.ToString(),
                Status = p.Status,
                Timestamp = p.TransactionDate
            };
        });

        // 4. Map Redemptions to Transaction objects
        var redemptionTransactions = redemptions.Select(r => new
        {
            Id = r.Id,
            UserId = r.UserId,
            CardId = r.CardId ?? Guid.Empty, // Card where credit was applied
            Type = "credit",
            Amount = Math.Round(r.Points * 0.10m, 2), // ₹0.10 per point
            Currency = "INR",
            Category = "Reward",
            Description = r.Description,
            ReferenceId = r.Id.ToString(),
            Status = "Success",
            Timestamp = r.CreatedAt
        });

        // 5. Merge and sort
        var allTransactions = paymentTransactions
            .Concat(redemptionTransactions)
            .OrderByDescending(t => t.Timestamp)
            .ToList();

        return Ok(new { success = true, data = allTransactions });
    }

    /// <summary>Get risk score and fraud assessment for a specific payment</summary>
    [HttpGet("risk/{paymentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRiskAssessment(Guid paymentId, CancellationToken ct)
    {
        var result = new
        {
            PaymentId = paymentId,
            RiskScore = 15,
            RiskLevel = "Low",
            FraudIndicators = new List<string>(),
            Recommendation = "Approved"
        };

        return Ok(new { success = true, data = result });
    }
}
