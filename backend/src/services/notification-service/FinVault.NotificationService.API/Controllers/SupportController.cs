using FinVault.NotificationService.Application.Commands.CreateTicket;
using FinVault.NotificationService.Application.Commands.ResolveTicket;
using FinVault.NotificationService.Application.Queries.GetAllTickets;
using FinVault.NotificationService.Application.Queries.GetTickets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinVault.NotificationService.API.Controllers;

[ApiController]
[Route("api/support")]
[Produces("application/json")]
public class SupportController : ControllerBase
{
    private readonly IMediator _mediator;

    public SupportController(IMediator mediator) => _mediator = mediator;

    /// <summary>DEBUG ONLY: Get all tickets without auth</summary>
    [HttpGet("debug/all")]
    [AllowAnonymous]
    public async Task<IActionResult> DebugGetAllTickets(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllTicketsQuery(), ct);
        return Ok(new { success = true, count = result.Count, data = result });
    }

    private Guid GetUserId() => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get all support tickets</summary>
    [HttpGet("tickets")]
    [Authorize]
    public async Task<IActionResult> GetTickets(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTicketsQuery(GetUserId()), ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Create a new support ticket</summary>
    [HttpPost("tickets")]
    [Authorize]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateTicketCommand(GetUserId(), request.Subject, request.Message), ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get all support tickets (Admin Only)</summary>
    [HttpGet("admin/tickets")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminTickets(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllTicketsQuery(), ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Resolve a support ticket with a comment (Admin Only)</summary>
    [HttpPost("admin/tickets/{id:guid}/resolve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResolveTicket(Guid id, [FromBody] ResolveTicketRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ResolveTicketCommand(id, request.Comment), ct);
        if (!result) return NotFound(new { success = false, message = "Ticket not found" });
        return Ok(new { success = true, message = "Ticket resolved successfully" });
    }
}

public record CreateTicketRequest(string Subject, string Message);
public record ResolveTicketRequest(string Comment);
