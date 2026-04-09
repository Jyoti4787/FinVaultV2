using MediatR;

namespace FinVault.IdentityService.Application.Commands.UploadProfilePicture;

public record UploadProfilePictureCommand(
    Guid UserId,
    string FileName,
    string ContentType,
    Stream ImageStream
) : IRequest<UploadProfilePictureResult>;

public record UploadProfilePictureResult(string ProfilePictureUrl);
