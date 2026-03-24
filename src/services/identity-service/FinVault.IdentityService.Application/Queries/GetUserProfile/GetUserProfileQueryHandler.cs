using FinVault.IdentityService.Domain.Interfaces;
using MediatR;

namespace FinVault.IdentityService.Application.Queries.GetUserProfile;

// WHAT DOES THIS HANDLER DO?
// Very simple — just one step
// Find user by ID → return their data
// No changes to database
// No events fired
// Pure READ operation

// Published by : MediatR routes here when GetUserProfileQuery sent
// Consumed by  : Nobody — returns profile data to controller
public class GetUserProfileQueryHandler
    : IRequestHandler<GetUserProfileQuery, GetUserProfileResult>
{
    private readonly IUserRepository _users;

    public GetUserProfileQueryHandler(IUserRepository users)
    {
        // _users is our connection to the database
        // via the IUserRepository interface
        _users = users;
    }

    public async Task<GetUserProfileResult> Handle(
        GetUserProfileQuery query,
        CancellationToken ct)
    {
        // Find user by ID
        // ?? means if null → throw exception
        // ExceptionMiddleware catches KeyNotFoundException
        // and returns 404 Not Found to the caller
        var user = await _users.GetByIdAsync(
            query.UserId, ct)
            ?? throw new KeyNotFoundException(
                "User not found.");

        // Return only the data the frontend needs
        // We do NOT return PasswordHash
        // We do NOT return IsActive
        // Only safe public data goes out
        return new GetUserProfileResult(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.IsEmailVerified,
            user.CreatedAt);
    }
}