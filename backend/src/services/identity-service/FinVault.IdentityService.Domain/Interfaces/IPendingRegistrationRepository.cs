using FinVault.IdentityService.Domain.Entities;

namespace FinVault.IdentityService.Domain.Interfaces;

public interface IPendingRegistrationRepository
{
    Task AddAsync(PendingRegistration record, CancellationToken ct);

    Task<PendingRegistration?> GetByEmailAsync(string email, CancellationToken ct);

    Task UpdateAsync(PendingRegistration record, CancellationToken ct);

    Task DeleteAsync(string email, CancellationToken ct);
}
