using FinVault.IdentityService.Application.Queries.GetUserProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinVault.IdentityService.API.Controllers;

// WHAT IS [Authorize]?
// This means the endpoint is PROTECTED
// User MUST send a valid JWT token in the header
// If no token → 401 Unauthorized automatically
// If expired token → 401 Unauthorized automatically
// The middleware checks the token before
// the controller even runs

[ApiController]
[Route("api/identity/users")]
[Produces("application/json")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>Get the currently logged in user profile</summary>
    [HttpGet("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile(
        CancellationToken ct)
    {
        // HOW DO WE KNOW WHICH USER IS LOGGED IN?
        // The JWT token contains claims (data)
        // One of those claims is the userId
        // We read it from User.Claims
        // User = the current logged in user
        //        ASP.NET automatically fills this
        //        from the JWT token

        // Try to read userId from "sub" claim
        // "sub" = subject = who this token is for
        // This is standard JWT claim name
        var userIdString =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException(
                "User ID not found in token.");

        var userId = Guid.Parse(userIdString);

        // Send query to MediatR
        // Handler fetches user from database
        var result = await _mediator.Send(
            new GetUserProfileQuery(userId), ct);

        return Ok(new { success = true, data = result });
    }
}