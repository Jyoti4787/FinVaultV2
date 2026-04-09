using FinVault.CardService.Application.Commands.AddCard;
using FinVault.CardService.Application.Commands.RemoveCard;
using FinVault.CardService.Application.Commands.SetDefaultCard;
using FinVault.CardService.Application.Commands.UpdateCardLimit;
using FinVault.CardService.Application.Commands.VerifyCard;
using FinVault.CardService.Application.Queries.GetCardById;
using FinVault.CardService.Application.Queries.GetCards;
using FinVault.CardService.Application.Queries.GetCardUtilization;
using FinVault.CardService.Application.Queries.RevealCard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinVault.CardService.API.Controllers;

// Receives : HTTP requests from Ocelot Gateway /api/cards/*
// Sends to : Angular SPA (card module)
[ApiController]
[Route("api/cards")]
[Produces("application/json")]
[Authorize]
public class CardsController : ControllerBase
{
    private readonly IMediator _mediator;
    public CardsController(IMediator mediator) => _mediator = mediator;

    // Read userId from JWT token — never from request body
    private Guid GetUserId() => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get all cards for the logged in user</summary>
    [HttpGet]
    public async Task<IActionResult> GetCards(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCardsQuery(GetUserId()), ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get a specific card by ID</summary>
    [HttpGet("{cardId:guid}")]
    public async Task<IActionResult> GetCard(Guid cardId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCardByIdQuery(cardId, GetUserId()), ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get utilization percentage for a card</summary>
    [HttpGet("{cardId:guid}/utilization")]
    public async Task<IActionResult> GetUtilization(Guid cardId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCardUtilizationQuery(cardId, GetUserId()), ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get total utilization across all user cards</summary>
    [HttpGet("utilization")]
    public async Task<IActionResult> GetTotalUtilization(CancellationToken ct)
    {
        var cards = await _mediator.Send(new GetCardsQuery(GetUserId()), ct);
        
        var totalLimit = cards.Sum(c => c.CreditLimit);
        var totalBalance = cards.Sum(c => c.OutstandingBalance);
        var utilizationPercent = totalLimit > 0 ? (totalBalance / totalLimit) * 100 : 0;

        var result = new
        {
            TotalCreditLimit = totalLimit,
            TotalOutstandingBalance = totalBalance,
            UtilizationPercent = Math.Round(utilizationPercent, 2),
            CardCount = cards.Count
        };

        return Ok(new { success = true, data = result });
    }

    /// <summary>Reveal full card number and CVV (temporary access)</summary>
    [HttpGet("{cardId:guid}/reveal")]
    public async Task<IActionResult> RevealCard(Guid cardId, CancellationToken ct)
    {
        var result = await _mediator.Send(new RevealCardQuery(cardId, GetUserId()), ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Add a new credit card</summary>
    [HttpPost]
    public async Task<IActionResult> AddCard(
        [FromBody] AddCardRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddCardCommand(
            GetUserId(), request.CardNumber, request.CardholderName,
            request.ExpiryMonth, request.ExpiryYear, request.Cvv,
            request.CreditLimit, request.BillingCycleStartDay, request.CorrelationId), ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Remove a card (soft delete)</summary>
    [HttpDelete("{cardId:guid}")]
    public async Task<IActionResult> RemoveCard(Guid cardId, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new RemoveCardCommand(cardId, GetUserId(), Guid.NewGuid()), ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Set this card as default</summary>
    [HttpPatch("{cardId:guid}/default")]
    public async Task<IActionResult> SetDefault(Guid cardId, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new SetDefaultCardCommand(cardId, GetUserId(), Guid.NewGuid()), ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Verify card with Rs.1 micro-auth</summary>
    [HttpPost("{cardId:guid}/verify")]
    public async Task<IActionResult> VerifyCard(Guid cardId, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new VerifyCardCommand(cardId, GetUserId(), Guid.NewGuid()), ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Update credit limit for a card</summary>
    [HttpPatch("{cardId:guid}/limit")]
    public async Task<IActionResult> UpdateLimit(
        Guid cardId, [FromBody] UpdateLimitRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateCardLimitCommand(cardId, GetUserId(),
                request.NewLimit, Guid.NewGuid()), ct);
        return Ok(new { success = true, data = result });
    }
    /// <summary>Get all cards in the system (Admin only)</summary>
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllCardsAdmin(CancellationToken ct)
    {
        var result = await _mediator.Send(new FinVault.CardService.Application.Queries.GetAllCardsAdmin.GetAllCardsAdminQuery(), ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Approve a card (Admin only)</summary>
    [HttpPost("admin/{cardId:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveCardAdmin(Guid cardId, CancellationToken ct)
    {
        var result = await _mediator.Send(new FinVault.CardService.Application.Commands.ApproveCardAdmin.ApproveCardAdminCommand(cardId), ct);
        return Ok(new { success = true, data = result });
    }
}

public record AddCardRequest(
    string CardNumber, string CardholderName,
    int ExpiryMonth, int ExpiryYear,
    string Cvv,
    decimal CreditLimit, int BillingCycleStartDay,
    Guid CorrelationId);

public record UpdateLimitRequest(decimal NewLimit);