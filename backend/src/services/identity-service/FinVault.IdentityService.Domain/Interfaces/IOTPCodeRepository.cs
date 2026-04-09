using FinVault.IdentityService.Domain.Entities;

namespace FinVault.IdentityService.Domain.Interfaces;

public interface IOTPCodeRepository
{
    /// <summary>Save a new OTP to the database.</summary>
    Task AddAsync(OTPCode code, CancellationToken ct);

    /// <summary>
    /// Find the most recent active (unused, unexpired) OTP
    /// for a user with a specific purpose.
    /// </summary>
    Task<OTPCode?> GetActiveCodeAsync(Guid userId, string purpose, CancellationToken ct);

    /// <summary>Mark an OTP as used so it cannot be verified again.</summary>
    Task MarkAsUsedAsync(Guid codeId, CancellationToken ct);

    /// <summary>
    /// Count how many OTPs were created for a user+purpose in the last window.
    /// Used for rate limiting — max 3 per 5 minutes.
    /// </summary>
    Task<int> CountRecentAsync(Guid userId, string purpose, TimeSpan window, CancellationToken ct);

    /// <summary>
    /// Invalidate (mark used) all active OTPs for a user+purpose.
    /// Called on resend so old codes can't be used after a new one is issued.
    /// </summary>
    Task InvalidateAllActiveAsync(Guid userId, string purpose, CancellationToken ct);
}
