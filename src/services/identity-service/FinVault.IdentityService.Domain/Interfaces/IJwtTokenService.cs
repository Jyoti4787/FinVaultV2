using FinVault.IdentityService.Domain.Entities;

namespace FinVault.IdentityService.Domain.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
