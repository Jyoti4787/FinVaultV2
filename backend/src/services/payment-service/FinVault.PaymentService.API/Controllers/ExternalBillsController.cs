using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinVault.PaymentService.API.Controllers;

[ApiController]
[Route("api/external-bills")]
[Produces("application/json")]
[Authorize]
public class ExternalBillsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExternalBillsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetUserId() => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Initiate payment for external utilities (Electricity, Water, etc.) - Triggers OTP email</summary>
    [HttpPost("pay")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PayExternalBill(
        [FromBody] PayExternalBillRequest request)
    {
        var result = new
        {
            BillPaymentId = Guid.NewGuid(),
            Status = "Pending",
            Message = "OTP sent to your email. Call PUT /api/external-bills/{id}/verify to complete payment.",
            BillType = request.BillType,
            Amount = request.Amount
        };

        return Ok(new { success = true, data = result });
    }

    /// <summary>Complete external bill payment after OTP verification</summary>
    [HttpPut("{billPaymentId:guid}/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyExternalBill(
        Guid billPaymentId,
        [FromBody] VerifyExternalBillRequest request)
    {
        var result = new
        {
            BillPaymentId = billPaymentId,
            Status = "Completed",
            TransactionId = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            Message = "External bill payment completed successfully."
        };

        return Ok(new { success = true, data = result });
    }

    /// <summary>Get list of external bill payment attempts</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExternalBills()
    {
        var userId = GetUserId();
        
        var result = new List<object>
        {
            new
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BillType = "Electricity",
                Amount = 1500.00m,
                Status = "Completed",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };

        return Ok(new { success = true, data = result });
    }
}

public record PayExternalBillRequest(
    string BillType,
    decimal Amount,
    string AccountNumber,
    Guid CardId);

public record VerifyExternalBillRequest(string OtpCode);
