// ==================================================================
// FILE : IPaymentOtpVerifier.cs
// LAYER: Domain (Interfaces)
// WHAT IS THIS?
// Abstracts the OTP verification call to identity-service.
// The Application layer depends only on this interface.
// The actual HTTP call is in Infrastructure/OtpVerification/.
// ==================================================================

namespace FinVault.PaymentService.Domain.Interfaces;

public interface IPaymentOtpVerifier
{
    /// <summary>
    /// Calls identity-service to verify an OTP code.
    /// Returns true if the OTP is valid, false otherwise.
    /// </summary>
    Task<bool> VerifyAsync(
        string email,
        string otpCode,
        string purpose,
        CancellationToken ct = default);
}
