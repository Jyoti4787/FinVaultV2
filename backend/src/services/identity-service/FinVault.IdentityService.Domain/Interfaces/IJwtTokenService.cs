using FinVault.IdentityService.Domain.Entities;

namespace FinVault.IdentityService.Domain.Interfaces;

// Contract for JWT token generation
// Implementation lives in Infrastructure layer
// Application layer only knows this interface
public interface IJwtTokenService
{
    // Generate a JWT access token for a user
    string GenerateAccessToken(User user);

    // Generate a random refresh token string
    string GenerateRefreshToken();
}