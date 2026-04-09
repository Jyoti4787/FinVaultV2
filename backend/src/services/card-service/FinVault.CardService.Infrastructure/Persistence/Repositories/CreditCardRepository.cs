using FinVault.CardService.Domain.Entities;
using FinVault.CardService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVault.CardService.Infrastructure.Persistence.Repositories;

public class CreditCardRepository : ICreditCardRepository
{
    private readonly CardDbContext _ctx;
    public CreditCardRepository(CardDbContext ctx) => _ctx = ctx;

    public async Task<List<CreditCard>> GetAllAsync(CancellationToken ct)
        => await _ctx.CreditCards
            .Include(x => x.Issuer)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task<CreditCard?> GetByIdAsync(
        Guid id, CancellationToken ct)
        => await _ctx.CreditCards
            .Include(x => x.Issuer)
            .FirstOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted, ct);

    public async Task<List<CreditCard>> GetByUserIdAsync(
        Guid userId, CancellationToken ct)
        => await _ctx.CreditCards
            .Include(x => x.Issuer)
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(
        Guid userId, string maskedNumber, CancellationToken ct)
        => await _ctx.CreditCards
            .AnyAsync(x =>
                x.UserId == userId &&
                x.MaskedNumber == maskedNumber &&
                !x.IsDeleted, ct);

    public async Task<Guid> AddAsync(
        CreditCard card, CancellationToken ct)
    {
        await _ctx.CreditCards.AddAsync(card, ct);
        await _ctx.SaveChangesAsync(ct);
        return card.Id;
    }

    public async Task UpdateAsync(
        CreditCard card, CancellationToken ct)
    {
        _ctx.CreditCards.Update(card);
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task<CreditCard?> GetDefaultCardAsync(
        Guid userId, CancellationToken ct)
        => await _ctx.CreditCards
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.IsDefault &&
                !x.IsDeleted, ct);

    public async Task ClearDefaultForUserAsync(
        Guid userId, CancellationToken ct)
        => await _ctx.CreditCards
            .Where(x => x.UserId == userId && x.IsDefault)
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.IsDefault, false), ct);
}