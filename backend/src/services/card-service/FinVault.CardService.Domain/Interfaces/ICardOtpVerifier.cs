namespace FinVault.CardService.Domain.Interfaces;

/// <summary>
/// Verifies OTP codes by calling the identity-service.
/// Used to secure card detail reveal operations.
/// </summary>
public interface ICardOtpVerifier
{
    /// <summary>
    /// Verify an OTP code against the identity service.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="otpCode">The 6-digit OTP code</param>
    /// <param name="purpose">Purpose of the OTP (e.g., "CardReveal")</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if OTP is valid, false otherwise</returns>
    Task<bool> VerifyAsync(string email, string otpCode, string purpose, CancellationToken ct = default);
}
