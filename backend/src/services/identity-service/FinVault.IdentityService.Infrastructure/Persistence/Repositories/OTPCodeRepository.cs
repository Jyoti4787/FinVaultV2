using FinVault.IdentityService.Domain.Entities;
using FinVault.IdentityService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVault.IdentityService.Infrastructure.Persistence.Repositories;

public class OTPCodeRepository : IOTPCodeRepository
{
    private readonly IdentityDbContext _ctx;

    public OTPCodeRepository(IdentityDbContext ctx) => _ctx = ctx;

    /// <summary>Save a new OTP to the database.</summary>
    public async Task AddAsync(OTPCode code, CancellationToken ct)
    {
        await _ctx.OTPCodes.AddAsync(code, ct);
        await _ctx.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Find the most recent active (unused + unexpired) OTP
    /// for a user with a specific purpose.
    /// </summary>
    public async Task<OTPCode?> GetActiveCodeAsync(
        Guid userId, string purpose, CancellationToken ct)
        => await _ctx.OTPCodes
            .Where(x =>
                x.UserId    == userId  &&
                x.Purpose   == purpose &&
                !x.IsUsed              &&
                x.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

    /// <summary>Mark a single OTP as used (prevents replay).</summary>
    public async Task MarkAsUsedAsync(Guid codeId, CancellationToken ct)
        => await _ctx.OTPCodes
            .Where(x => x.Id == codeId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.IsUsed, true), ct);

    /// <summary>
    /// Rate limiting — count OTPs created for this user+purpose
    /// within the given time window.
    /// Max 3 per 5 minutes enforced in the handler.
    /// </summary>
    public async Task<int> CountRecentAsync(
        Guid userId, string purpose, TimeSpan window, CancellationToken ct)
    {
        var since = DateTimeOffset.UtcNow - window;
        return await _ctx.OTPCodes
            .CountAsync(x =>
                x.UserId   == userId  &&
                x.Purpose  == purpose &&
                x.CreatedAt >= since, ct);
    }

    /// <summary>
    /// Resend support — invalidate all active OTPs for this user+purpose
    /// before issuing a new one. Prevents old codes from working after resend.
    /// </summary>
    public async Task InvalidateAllActiveAsync(
        Guid userId, string purpose, CancellationToken ct)
        => await _ctx.OTPCodes
            .Where(x =>
                x.UserId   == userId  &&
                x.Purpose  == purpose &&
                !x.IsUsed             &&
                x.ExpiresAt > DateTimeOffset.UtcNow)
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.IsUsed, true), ct);
}
