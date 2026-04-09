namespace FinVault.IdentityService.Domain.Entities;

/// <summary>
/// Holds unverified registration attempts with complete user data.
/// A real User record is only created AFTER email OTP is confirmed.
/// </summary>
public class PendingRegistration
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    // Store user registration data temporarily
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // OTP code hash (BCrypt) — verified before allowing full register
    public string OtpCodeHash { get; set; } = string.Empty;

    /// True after the user successfully enters the correct OTP
    public bool EmailVerified { get; set; } = false;

    /// Pending record expires after 15 minutes
    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
