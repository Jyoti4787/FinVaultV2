using FinVault.IdentityService.Domain.Entities;

namespace FinVault.IdentityService.Domain.Interfaces;

// Published by : Nobody — just a contract
// Consumed by  : SendOTPCommandHandler (Add)
//                VerifyOTPCommandHandler (GetActiveCode, MarkAsUsed)
//                ResetPasswordCommandHandler (GetActiveCode, MarkAsUsed)
public interface IOTPCodeRepository
{
    // Save a new OTP code to the database
    // Called when user requests an OTP
    Task AddAsync(OTPCode code, CancellationToken ct);

    // Find an active OTP for a user with a specific purpose
    // Active means: not used AND not expired
    // Purpose = "Login" or "Payment" or "PasswordReset"
    Task<OTPCode?> GetActiveCodeAsync(
        Guid userId,
        string purpose,
        CancellationToken ct);

    // Mark an OTP as used after it is verified
    // Once marked as used it cannot be verified again
    // Prevents someone from using same OTP twice
    Task MarkAsUsedAsync(Guid codeId, CancellationToken ct);
}