using MediatR;

namespace FinVault.IdentityService.Application.Commands.LoginUser;

// WHAT HAPPENS WHEN YOU LOGIN?
// You type email + password in the app
// App sends it here as a "command"
// Think of it like filling a form and submitting it
// The form has 3 fields — email, password, correlationId

// Published by : AuthController POST /api/identity/auth/login
// Consumed by  : LoginUserCommandHandler
public record LoginUserCommand(
    // The email the user typed
    string Email,

    // The password the user typed
    // We NEVER store this — we only use it to check
    string Password,

    // A tracking number for this request
    // Like a courier tracking number
    // Helps you trace what happened across all services
    Guid CorrelationId

// IRequest<LoginUserResult> means
// "after this command runs, give me back a LoginUserResult"
) : IRequest<LoginUserResult>;

// WHAT COMES BACK after successful login?
public record LoginUserResult(
    // The JWT token — user puts this in every request header
    // Lasts only 15 MINUTES for security
    string AccessToken,

    // The refresh token — used to get a new JWT silently
    // Lasts 7 DAYS
    // Like a "remember me" token
    string RefreshToken,

    // Who logged in
    Guid UserId,
    string Email,

    // What role — User / Admin / SupportAgent
    // Frontend uses this to show/hide menu items
    string Role,

    // When does the JWT expire?
    // Frontend uses this to know when to refresh
    DateTimeOffset ExpiresAt);