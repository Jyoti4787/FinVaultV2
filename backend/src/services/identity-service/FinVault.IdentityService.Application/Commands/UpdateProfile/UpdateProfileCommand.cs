using MediatR;

namespace FinVault.IdentityService.Application.Commands.UpdateProfile;

public record UpdateProfileCommand(
    Guid UserId,
    string? FirstName,
    string? LastName,
    string? PhoneNumber
) : IRequest<UpdateProfileResult>;

public record UpdateProfileResult(
    string Message,
    string FirstName,
    string LastName,
    string? PhoneNumber);
