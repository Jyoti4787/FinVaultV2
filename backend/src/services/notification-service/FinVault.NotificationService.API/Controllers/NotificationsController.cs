// ==================================================================
// FILE : NotificationsController.cs
// LAYER: API (Controllers)
// PATH : notification-service/FinVault.NotificationService.API/Controllers/
// ==================================================================

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FinVault.NotificationService.Application.Queries.GetNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinVault.NotificationService.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Produces("application/json")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get all system alerts for the logged-in user</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                        ?? User.FindFirstValue("sub")
                        ?? throw new UnauthorizedAccessException();

        var result = await _mediator.Send(
            new GetNotificationsQuery(Guid.Parse(userIdString)));

        return Ok(new { success = true, data = result });
    }
}
