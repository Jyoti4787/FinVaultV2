using FinVault.IdentityService.Domain.Entities;
using FinVault.IdentityService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVault.IdentityService.Infrastructure.Persistence.Repositories;

// Implements IRefreshTokenRepository from Domain layer
// Does actual database operations for refresh tokens
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IdentityDbContext _ctx;

    public RefreshTokenRepository(IdentityDbContext ctx)
        => _ctx = ctx;

    // Find token by its string value
    // Used when user submits their refresh token
    // to get a new JWT
    public async Task<RefreshToken?> GetByTokenAsync(
        string token, CancellationToken ct)
        => await _ctx.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.Token == token, ct);

    // Save new refresh token
    // Called after every login
    public async Task AddAsync(
        RefreshToken token, CancellationToken ct)
    {
        await _ctx.RefreshTokens.AddAsync(token, ct);
        await _ctx.SaveChangesAsync(ct);
    }

    // Cancel one specific token
    // Called after token is used to get new JWT
    // Old token must die immediately
    public async Task RevokeAsync(
        Guid tokenId, CancellationToken ct)
    {
        var token = await _ctx.RefreshTokens
            .FindAsync([tokenId], ct);

        // If token not found just return
        // Nothing to revoke
        if (token is null) return;

        token.IsRevoked = true;
        await _ctx.SaveChangesAsync(ct);
    }

    // Cancel ALL tokens for a user
    // Called when user resets password
    // ExecuteUpdateAsync is faster than loading
    // all tokens into memory and updating one by one
    // It runs a single UPDATE SQL statement
    // UPDATE RefreshTokens SET IsRevoked = 1
    // WHERE UserId = @userId AND IsRevoked = 0
    public async Task RevokeAllForUserAsync(
        Guid userId, CancellationToken ct)
    {
        await _ctx.RefreshTokens
            .Where(x =>
                x.UserId == userId &&
                !x.IsRevoked)
            .ExecuteUpdateAsync(
                s => s.SetProperty(
                    x => x.IsRevoked, true), ct);
    }
}