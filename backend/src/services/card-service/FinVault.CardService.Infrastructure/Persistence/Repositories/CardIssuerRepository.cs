using FinVault.CardService.Domain.Entities;
using FinVault.CardService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVault.CardService.Infrastructure.Persistence.Repositories;

public class CardIssuerRepository : ICardIssuerRepository
{
    private readonly CardDbContext _ctx;
    public CardIssuerRepository(CardDbContext ctx) => _ctx = ctx;

    public async Task<List<CardIssuer>> GetAllAsync(
        CancellationToken ct)
        => await _ctx.CardIssuers.ToListAsync(ct);

    public async Task<CardIssuer?> GetByNameAsync(
        string name, CancellationToken ct)
        => await _ctx.CardIssuers
            .FirstOrDefaultAsync(x => x.Name == name, ct);
}