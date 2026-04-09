using FinVault.CardService.Domain.Entities;

namespace FinVault.CardService.Domain.Interfaces;

// Contract for all credit card database operations
// Implemented by CreditCardRepository in Infrastructure
public interface ICreditCardRepository
{
    Task<List<CreditCard>> GetAllAsync(CancellationToken ct);
    Task<CreditCard?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<CreditCard>> GetByUserIdAsync(Guid userId, CancellationToken ct);
    Task<bool> ExistsAsync(Guid userId, string maskedNumber, CancellationToken ct);
    Task<Guid> AddAsync(CreditCard card, CancellationToken ct);
    Task UpdateAsync(CreditCard card, CancellationToken ct);
    Task<CreditCard?> GetDefaultCardAsync(Guid userId, CancellationToken ct);
    Task ClearDefaultForUserAsync(Guid userId, CancellationToken ct);
}