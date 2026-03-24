namespace FinVault.IdentityService.Domain.Entities;

public class OTPCode
{
    public Guid Id { get; set; }

    // Which user requested this OTP?
    public Guid UserId { get; set; }

    // The 6-digit code — but HASHED
    // We never store "123456" directly
    // We store BCrypt hash of it — same reason as password
    public string CodeHash { get; set; } = string.Empty;

    // WHY was this OTP created?
    // "Login"        = logging in from new device
    // "Payment"      = confirming a big payment
    // "PasswordReset"= resetting password
    // We need this because one user might have
    // multiple OTPs for different purposes at same time
    public string Purpose { get; set; } = string.Empty;

    // OTP expires in 5 minutes
    // After that it cannot be used
    public DateTimeOffset ExpiresAt { get; set; }

    // Has this OTP been used already?
    // Once used → IsUsed = true → cannot be used again
    // This prevents replay attacks
    public bool IsUsed { get; set; } = false;

    public DateTimeOffset CreatedAt { get; set; }

    // Navigation property — links back to the User
    public User User { get; set; } = null!;
}