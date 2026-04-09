using FinVault.IdentityService.Domain.Entities;

namespace FinVault.IdentityService.Domain.Interfaces;

// Published by : Nobody — just a contract
// Consumed by  : LoginUserCommandHandler (Add)
//                RefreshTokenCommandHandler (GetByToken, Revoke, Add)
//                ResetPasswordCommandHandler (RevokeAllForUser)
public interface IRefreshTokenRepository
{
    // Find a refresh token by its string value
    // Used when user wants a new JWT using their refresh token
    Task<RefreshToken?> GetByTokenAsync(
        string token, CancellationToken ct);

    // Save a new refresh token
    // Called after every successful login
    Task AddAsync(RefreshToken token, CancellationToken ct);

    // Cancel one specific token
    // Called when user uses it to get a new JWT
    // Old token must die so it cannot be reused
    Task RevokeAsync(Guid tokenId, CancellationToken ct);

    // Cancel ALL tokens for a user
    // Called when user resets password
    // All existing sessions become invalid
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct);
}