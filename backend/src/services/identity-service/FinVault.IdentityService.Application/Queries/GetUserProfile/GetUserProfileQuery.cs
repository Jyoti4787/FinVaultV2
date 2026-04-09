using MediatR;

namespace FinVault.IdentityService.Application.Queries.GetUserProfile;

// WHAT IS A QUERY?
// A Query is different from a Command
// Command = DO something (change data)
// Query  = GET something (read data, change nothing)
//
// Think of it like this:
// Command = "Please register me" (creates a user)
// Query   = "Please show me my profile" (just reads)
//
// Why separate them?
// This is called CQRS pattern
// Read operations are faster when separated from writes
// You can add caching to reads without affecting writes
// Each handler does ONE thing only

// Published by : UsersController GET /api/identity/users/profile
// Consumed by  : GetUserProfileQueryHandler
public record GetUserProfileQuery(
    // Whose profile do we want?
    // This comes from the JWT token
    // Controller reads userId FROM the token
    // So user cannot fake someone else's userId
    Guid UserId
) : IRequest<GetUserProfileResult>;

// What comes back — the user's profile data
public record GetUserProfileResult(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,

    // Role = User / Admin / SupportAgent
    // Frontend uses this to show/hide features
    string Role,

    // Has the user clicked the verification link?
    // If false → frontend shows "please verify email" banner
    bool IsEmailVerified,

    // When did they join?
    DateTimeOffset CreatedAt);