namespace FinVault.CardService.Domain.Entities;

// WHAT IS THIS?
// This is the main table — CreditCards
// Each row = one credit card belonging to one user
// We NEVER store the full card number
// We only store the last 4 digits (masked)
// CVV is NEVER stored at all

public class CreditCard
{
    public Guid Id { get; set; }

    // Which user owns this card?
    // Cross-service ID — no foreign key to identity-service
    // Just a plain Guid
    public Guid UserId { get; set; }

    // e.g. "**** **** **** 1234"
    // Full number never stored — PCI-DSS rule
    public string MaskedNumber { get; set; } = string.Empty;

    // Name printed on the card
    public string CardholderName { get; set; } = string.Empty;

    // Expiry month — 1 to 12
    public int ExpiryMonth { get; set; }

    // Expiry year — e.g. 2027
    public int ExpiryYear { get; set; }

    // Which bank/network issued this card?
    // Links to CardIssuers table
    public Guid IssuerId { get; set; }

    // User-entered credit limit in INR
    public decimal CreditLimit { get; set; }

    // How much is currently owed
    // Updated every time a payment is made
    // Starts at 0
    public decimal OutstandingBalance { get; set; } = 0;

    // Which day of the month does billing cycle start?
    // 1 to 28 — we avoid 29,30,31 because not all months have them
    public int BillingCycleStartDay { get; set; }

    // Is this the user's default card?
    // Only ONE card per user can be default
    public bool IsDefault { get; set; } = false;

    // Has this card been verified with Rs.1 micro-auth?
    // Only verified cards can process payments
    public bool IsVerified { get; set; } = false;

    // Soft delete — we never hard delete cards
    // Because payment history still references them
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    // Navigation property — gives us the full CardIssuer object
    public CardIssuer Issuer { get; set; } = null!;
}