namespace FinVault.CardService.Domain.Entities;

// WHAT IS THIS?
// This is a lookup table — seeded once at startup
// It has 4 rows: Visa, MasterCard, Amex, RuPay
// We detect the issuer automatically from the card number
// using BIN prefixes (first 6 digits of card number)

public class CardIssuer
{
    public Guid Id { get; set; }

    // "Visa" or "MasterCard" or "Amex" or "RuPay"
    public string Name { get; set; } = string.Empty;

    // How many digits does this card have?
    // Visa/MC/RuPay = 16 digits
    // Amex = 15 digits
    public int CardLength { get; set; }

    // Comma-separated BIN prefixes
    // e.g. "4" for Visa (all Visa cards start with 4)
    // e.g. "51,52,53,54,55" for MasterCard
    // We use this to auto-detect the issuer
    public string BinPrefixes { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}