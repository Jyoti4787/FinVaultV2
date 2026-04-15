using System.Security.Claims;
using MediatR;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinVault.PaymentService.API.Controllers;

[ApiController]
[Route("api/external-bills")]
[Produces("application/json")]
[Authorize]
public class ExternalBillsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly FinVault.PaymentService.Infrastructure.Persistence.PaymentDbContext _dbContext;
    private readonly HttpClient _httpClient;

    public ExternalBillsController(
        IMediator mediator, 
        IPublishEndpoint publishEndpoint,
        FinVault.PaymentService.Infrastructure.Persistence.PaymentDbContext dbContext,
        IHttpClientFactory httpClientFactory)
    {
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _httpClient = httpClientFactory.CreateClient();
    }

    private Guid GetUserId() => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Pay an external bill/recharge/UPI — saved directly to DB</summary>
    [HttpPost("pay")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PayExternalBill(
        [FromBody] PayExternalBillRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        
        var userEmail = User.FindFirstValue(ClaimTypes.Email)
                     ?? User.FindFirstValue("email")
                     ?? throw new UnauthorizedAccessException("Email claim missing from token.");

        // 1. VERIFY OTP
        var identityUrl = "http://identity-service:8080/api/identity/auth/mfa/verify";
        var verifyPayload = new { Email = userEmail, Code = request.OtpCode, Purpose = "Payment" };

        var otpResponse = await _httpClient.PostAsJsonAsync(identityUrl, verifyPayload, ct);
        if (!otpResponse.IsSuccessStatusCode)
        {
            return BadRequest(new { success = false, message = "Invalid or expired OTP." });
        }
        var otpResult = await otpResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(ct);
        if (!otpResult.TryGetProperty("data", out var data) || 
            !data.TryGetProperty("isValid", out var validToken) || 
            !validToken.GetBoolean())
        {
            return BadRequest(new { success = false, message = "Invalid or expired OTP." });
        }

        var payment = new FinVault.PaymentService.Domain.Entities.Payment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CardId = request.CardId,
            Amount = request.Amount,
            Currency = "INR",
            Status = "Success",
            TransactionDate = DateTime.UtcNow,
            // Encode bill type and account in ExternalTransactionId as "BillType|AccountNumber"
            ExternalTransactionId = $"{request.BillType}|{request.AccountNumber}"
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        // Award reward points: 1 point per ₹10 spent (min 1 point)
        int pointsEarned = Math.Max(1, (int)(request.Amount / 10));
        var reward = new FinVault.PaymentService.Domain.Entities.RewardPoint
        {
            UserId = userId,
            PaymentId = payment.Id,
            Points = pointsEarned,
            Description = $"{request.BillType} — ₹{request.Amount}",
            Type = "Earned"
        };
        _dbContext.RewardPoints.Add(reward);
        await _dbContext.SaveChangesAsync();

        // Notify other services (e.g., Card Service to update balance)
        await _publishEndpoint.Publish(new FinVault.Shared.Contracts.Messages.PaymentCompletedMessage(
            payment.Id,
            userId,
            payment.CardId,
            payment.Amount,
            Guid.NewGuid(),
            request.BillType
        ));

        return Ok(new
        {
            success = true,
            data = new
            {
                PaymentId = payment.Id,
                Status = "Success",
                BillType = request.BillType,
                Amount = request.Amount,
                PointsEarned = pointsEarned,
                Message = $"{request.BillType} payment of ₹{request.Amount} completed! You earned {pointsEarned} reward points 🎉"
            }
        });
    }

    /// <summary>Get external bill payment history for the current user</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetExternalBillHistory()
    {
        var userId = GetUserId();

        // Fetch all payments for this user where ExternalTransactionId has a '|' (= external bills)
        var externalPayments = await _dbContext.Payments
            .Where(p => p.UserId == userId && p.ExternalTransactionId != null && p.ExternalTransactionId.Contains("|"))
            .OrderByDescending(p => p.TransactionDate)
            .ToListAsync(); // Evaluate client-side to prevent LINQ String.Split Exception

        var result = externalPayments.Select(p => new
            {
                p.Id,
                p.CardId,
                p.Amount,
                p.Currency,
                p.Status,
                p.TransactionDate,
                BillType = p.ExternalTransactionId!.Split('|')[0],
                AccountNumber = p.ExternalTransactionId!.Split('|')[1]
            }).ToList();

        return Ok(new { success = true, data = result });
    }
}

public record PayExternalBillRequest(
    string BillType,
    decimal Amount,
    string AccountNumber,
    Guid CardId,
    string OtpCode);

