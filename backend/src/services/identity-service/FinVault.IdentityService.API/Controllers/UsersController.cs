using FinVault.IdentityService.Application.Commands.UploadProfilePicture;
using FinVault.IdentityService.Application.Queries.GetUserProfile;
using FinVault.IdentityService.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinVault.IdentityService.API.Controllers;

[ApiController]
[Route("api/identity/users")]
[Produces("application/json")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IProfilePictureRepository _pictures;

    public UsersController(IMediator mediator, IProfilePictureRepository pictures)
    {
        _mediator = mediator;
        _pictures = pictures;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());

    /// <summary>Get the currently logged in user profile</summary>
    [HttpGet("profile")]
    [HttpGet("me")] // Alias for API reference compatibility
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserProfileQuery(GetUserId()), ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Upload a profile picture — stored in SQL Server as binary</summary>
    [HttpPost("profile/picture")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { success = false, message = "No file provided." });

        var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowed.Contains(file.ContentType))
            return BadRequest(new { success = false, message = "Only JPEG, PNG and WebP allowed." });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { success = false, message = "Max file size is 5 MB." });

        await using var stream = file.OpenReadStream();
        var result = await _mediator.Send(
            new UploadProfilePictureCommand(GetUserId(), file.FileName, file.ContentType, stream), ct);

        return Ok(new { success = true, data = result });
    }

    /// <summary>Serve profile picture by userId — no auth required</summary>
    [HttpGet("profile/picture/{userId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProfilePicture(Guid userId, CancellationToken ct)
    {
        try
        {
            var (stream, contentType) = await _pictures.DownloadAsync(userId.ToString(), ct);
            return File(stream, contentType);
        }
        catch (FileNotFoundException) { return NotFound(); }
    }
}


public record UpdateProfileRequest(
    string? FirstName,
    string? LastName,
    string? PhoneNumber);
