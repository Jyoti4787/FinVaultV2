//A JWT token lasts only 15 minutes for security. But we don't want users to login again every 15 minutes. 
//So we give them a Refresh Token that lasts 7 days — they use it to silently get a new JWT without logging in again.


namespace FinVault.IdentityService.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }

    // Which user does this token belong to?
    // This is a FOREIGN KEY — links to Users table
    public Guid UserId { get; set; }

    // The actual token string
    // We store a HASHED version for security
    public string Token { get; set; } = string.Empty;

    // When does this token expire?
    // After 7 days it becomes invalid automatically
    public DateTimeOffset ExpiresAt { get; set; }

    // Has this token been cancelled?
    // When user logs out → IsRevoked = true
    // When user resets password → IsRevoked = true for ALL tokens
    public bool IsRevoked { get; set; } = false;

    public DateTimeOffset CreatedAt { get; set; }

    // Navigation property
    // This means — "give me the full User object for this token"
    // EF Core uses this to do JOIN queries automatically
    // null! means "trust me, this will never be null at runtime"
    public User User { get; set; } = null!;
}