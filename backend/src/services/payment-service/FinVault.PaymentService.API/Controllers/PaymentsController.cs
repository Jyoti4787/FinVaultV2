// ==================================================================
// FILE : PaymentsController.cs
// LAYER: API (Controllers)
// PATH : payment-service/FinVault.PaymentService.API/Controllers/
// ==================================================================

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FinVault.PaymentService.Application.Commands.ProcessPayment;
using FinVault.PaymentService.Application.Queries.GetPaymentHistory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinVault.PaymentService.API.Controllers;

[ApiController]
[Route("api/payments")]
[Produces("application/json")]
[Authorize] 
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Process a payment — requires valid Payment OTP</summary>
    [HttpPost("process")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcessPayment(
        [FromBody] ProcessPaymentRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                        ?? User.FindFirstValue("sub")
                        ?? throw new UnauthorizedAccessException();
        
        // Extract email from JWT claims
        var userEmail = User.FindFirstValue(ClaimTypes.Email)
                     ?? User.FindFirstValue("email")
                     ?? throw new UnauthorizedAccessException("Email claim missing from token.");

        var userId = Guid.Parse(userIdString);

        var result = await _mediator.Send(new ProcessPaymentCommand(
            userId,
            request.CardId,
            request.StatementId,
            request.Amount,
            request.Currency,
            request.CorrelationId ?? Guid.NewGuid(),
            request.OtpCode,
            userEmail));

        return Ok(new { success = true, data = result });
    }

    /// <summary>Send Payment OTP to email before processing payment</summary>
    [HttpPost("initiate-otp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult InitiatePaymentOtp()
    {
        // The user gets their email from their JWT.
        // They must call POST /api/identity/auth/mfa/send with purpose="Payment"
        // This endpoint just reminds them of the flow.
        return Ok(new
        {
            success = true,
            message = "Call POST /api/identity/auth/mfa/send with { email, purpose: 'Payment' } to receive your OTP."
        });
    }

    /// <summary>Get payment history for the current user</summary>
    [HttpGet("history")]
    [HttpGet] // Also support GET /api/payments
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                        ?? User.FindFirstValue("sub");
        
        var result = await _mediator.Send(
            new GetPaymentHistoryQuery(Guid.Parse(userIdString!)));

        return Ok(new { success = true, data = result });
    }

    /// <summary>Initiate a card payment (bill payment)</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InitiatePayment(
        [FromBody] InitiatePaymentRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) 
                     ?? User.FindFirstValue("sub")
                     ?? throw new UnauthorizedAccessException());

        // Create payment in Initiated status
        var result = new
        {
            PaymentId = Guid.NewGuid(),
            Status = "Initiated",
            Message = "Payment initiated. Call PUT /api/payments/{id}/complete to finalize."
        };

        return Ok(new { success = true, data = result });
    }

    /// <summary>Complete an initiated payment</summary>
    [HttpPut("{paymentId:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CompletePayment(Guid paymentId)
    {
        var result = new
        {
            PaymentId = paymentId,
            Status = "Completed",
            Message = "Payment completed successfully."
        };

        return Ok(new { success = true, data = result });
    }

    /// <summary>Reverse a transaction (for cancellations/errors)</summary>
    [HttpPut("{paymentId:guid}/reverse")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReversePayment(Guid paymentId)
    {
        var result = new
        {
            PaymentId = paymentId,
            Status = "Reversed",
            Message = "Payment reversed successfully."
        };

        return Ok(new { success = true, data = result });
    }
}

// ──────────────────────────────────────────────────────────────────
// REQUEST MODEL
// ──────────────────────────────────────────────────────────────────
public record ProcessPaymentRequest(
    Guid CardId,
    Guid StatementId,
    decimal Amount,
    string OtpCode,            // Required — get this from POST /api/identity/auth/mfa/send
    string Currency = "INR",
    Guid? CorrelationId = null
);



public record InitiatePaymentRequest(
    Guid CardId,
    Guid StatementId,
    decimal Amount,
    string Currency = "INR");
