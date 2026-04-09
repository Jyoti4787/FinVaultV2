using FinVault.IdentityService.Domain.Entities;
using FinVault.IdentityService.Domain.Interfaces;
using FinVault.IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinVault.IdentityService.Infrastructure.Persistence.Repositories;

public class PendingRegistrationRepository : IPendingRegistrationRepository
{
    private readonly IdentityDbContext _db;

    public PendingRegistrationRepository(IdentityDbContext db)
        => _db = db;

    public async Task AddAsync(PendingRegistration record, CancellationToken ct)
    {
        _db.PendingRegistrations.Add(record);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PendingRegistration?> GetByEmailAsync(string email, CancellationToken ct)
        => await _db.PendingRegistrations
            .Where(p => p.Email == email.ToLowerInvariant() && p.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task UpdateAsync(PendingRegistration record, CancellationToken ct)
    {
        _db.PendingRegistrations.Update(record);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(string email, CancellationToken ct)
    {
        var records = await _db.PendingRegistrations
            .Where(p => p.Email == email.ToLowerInvariant())
            .ToListAsync(ct);
        _db.PendingRegistrations.RemoveRange(records);
        await _db.SaveChangesAsync(ct);
    }
}
