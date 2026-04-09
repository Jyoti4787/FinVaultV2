using FinVault.CardService.Domain.Entities;

namespace FinVault.CardService.Domain.Interfaces;

// Contract for card issuer lookup operations
public interface ICardIssuerRepository
{
    Task<List<CardIssuer>> GetAllAsync(CancellationToken ct);
    Task<CardIssuer?> GetByNameAsync(string name, CancellationToken ct);
}