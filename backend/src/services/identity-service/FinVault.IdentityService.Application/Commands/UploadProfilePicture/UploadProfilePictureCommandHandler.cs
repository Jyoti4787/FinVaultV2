using FinVault.IdentityService.Domain.Interfaces;
using MediatR;

namespace FinVault.IdentityService.Application.Commands.UploadProfilePicture;

public class UploadProfilePictureCommandHandler
    : IRequestHandler<UploadProfilePictureCommand, UploadProfilePictureResult>
{
    private readonly IProfilePictureRepository _pictures;
    private readonly IUserRepository _users;

    public UploadProfilePictureCommandHandler(
        IProfilePictureRepository pictures,
        IUserRepository users)
    {
        _pictures = pictures;
        _users = users;
    }

    public async Task<UploadProfilePictureResult> Handle(
        UploadProfilePictureCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Upload image to MongoDB GridFS — returns a URL
        var url = await _pictures.UploadAsync(
            request.UserId,
            request.FileName,
            request.ContentType,
            request.ImageStream,
            cancellationToken);

        // 2. Save the URL back to SQL Server on the User row
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        user.ProfilePictureUrl = url;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _users.UpdateAsync(user, cancellationToken);

        return new UploadProfilePictureResult(url);
    }
}
