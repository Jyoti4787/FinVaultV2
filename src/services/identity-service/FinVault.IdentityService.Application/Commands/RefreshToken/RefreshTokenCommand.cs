using MediatR;

namespace FinVault.IdentityService.Application.Commands.RefreshToken;

// WHEN IS THIS USED?
// Every 15 minutes the JWT expires
// Angular automatically calls this endpoint
// Sends the refresh token → gets a new JWT
// User never has to login again for 7 days
// This all happens silently in the background

// Published by : AuthController POST /api/identity/auth/refresh
// Consumed by  : RefreshTokenCommandHandler
public record RefreshTokenCommand(
    // The 7-day refresh token the user has
    string Token,

    Guid CorrelationId
) : IRequest<RefreshTokenResult>;

// What comes back — a brand new JWT pair
public record RefreshTokenResult(
    // New JWT access token — good for 15 more minutes
    string AccessToken,

    // New refresh token — old one is cancelled
    // This is called "token rotation"
    // Each use gives you a new refresh token
    // So if someone steals it, it stops working after one use
    string RefreshToken,

    DateTimeOffset ExpiresAt);