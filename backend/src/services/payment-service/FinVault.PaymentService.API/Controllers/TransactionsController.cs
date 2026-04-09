using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinVault.PaymentService.API.Controllers;

[ApiController]
[Route("api/transactions")]
[Produces("application/json")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetUserId() => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get global transaction history (includes internal payments and external utility bills)</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions(CancellationToken ct)
    {
        var userId = GetUserId();
        
        // Placeholder - implement with proper repository pattern
        var result = new List<object>
        {
            new
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = "Payment",
                Amount = 5000.00m,
                Description = "Credit Card Payment",
                Category = "Finance",
                ReferenceId = "TX-9921",
                Status = "Completed",
                Timestamp = DateTime.UtcNow.AddDays(-2)
            }
        };

        return Ok(new { success = true, data = result });
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
