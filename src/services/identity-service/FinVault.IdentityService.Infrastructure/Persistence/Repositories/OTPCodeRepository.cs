using FinVault.IdentityService.Domain.Entities;
using FinVault.IdentityService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVault.IdentityService.Infrastructure.Persistence.Repositories;

// Implements IOTPCodeRepository from Domain layer
// Does actual database operations for OTP codes
public class OTPCodeRepository : IOTPCodeRepository
{
    private readonly IdentityDbContext _ctx;

    public OTPCodeRepository(IdentityDbContext ctx)
        => _ctx = ctx;

    // Save new OTP to database
    // Called when user requests an OTP
    public async Task AddAsync(
        OTPCode code, CancellationToken ct)
    {
        await _ctx.OTPCodes.AddAsync(code, ct);
        await _ctx.SaveChangesAsync(ct);
    }

    // Find an active OTP for a user with a specific purpose
    // Active means ALL THREE conditions must be true:
    // 1. UserId matches
    // 2. Purpose matches (Login/Payment/PasswordReset)
    // 3. Not already used (IsUsed = false)
    // 4. Not expired (ExpiresAt > now)
    // OrderByDescending = get the NEWEST one
    // In case user clicked "resend OTP" multiple times
    public async Task<OTPCode?> GetActiveCodeAsync(
        Guid userId,
        string purpose,
        CancellationToken ct)
        => await _ctx.OTPCodes
            .Where(x =>
                x.UserId   == userId   &&
                x.Purpose  == purpose  &&
                !x.IsUsed              &&
                x.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

    // Mark OTP as used after verification
    // ExecuteUpdateAsync = single SQL UPDATE
    // Much faster than load → change → save
    public async Task MarkAsUsedAsync(
        Guid codeId, CancellationToken ct)
    {
        await _ctx.OTPCodes
            .Where(x => x.Id == codeId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(
                    x => x.IsUsed, true), ct);
    }
}