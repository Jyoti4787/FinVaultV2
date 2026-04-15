using FinVault.IdentityService.Domain.Interfaces;
using MediatR;

namespace FinVault.IdentityService.Application.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UpdateProfileResult>
{
    private readonly IUserRepository _users;

    public UpdateProfileCommandHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<UpdateProfileResult> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.UserId, ct)
            ?? throw new InvalidOperationException("User not found");

        // Update only the fields that are provided
        // Email cannot be changed for security reasons
        if (!string.IsNullOrWhiteSpace(request.FirstName))
            user.FirstName = request.FirstName.Trim();

        if (!string.IsNullOrWhiteSpace(request.LastName))
            user.LastName = request.LastName.Trim();

        // Update phone number (can be null to clear it)
        if (request.PhoneNumber != null)
            user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) 
                ? null 
                : request.PhoneNumber.Trim();

        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _users.UpdateAsync(user, ct);

        return new UpdateProfileResult(
            "Profile updated successfully",
            user.FirstName,
            user.LastName,
            user.PhoneNumber);
    }
}
