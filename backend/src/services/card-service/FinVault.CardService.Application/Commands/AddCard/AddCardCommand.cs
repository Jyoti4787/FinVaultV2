using MediatR;

namespace FinVault.CardService.Application.Commands.AddCard;

// Published by : CardsController POST /api/cards
// Consumed by  : AddCardCommandHandler
public record AddCardCommand(
    Guid UserId,

    // Full 16-digit card number — only used for:
    // 1. Luhn validation (check it's a real card number)
    // 2. Detecting the issuer (Visa/MC/Amex/RuPay)
    // 3. Creating the masked version "**** **** **** 1234"
    // We NEVER save this to the database
    string CardNumber,

    string CardholderName,
    int ExpiryMonth,
    int ExpiryYear,

    // CVV — validated for format (3-4 digits) but NEVER stored (PCI-DSS)
    string Cvv,

    // User-entered credit limit in INR
    decimal CreditLimit,

    // Which day billing cycle starts (1-28)
    int BillingCycleStartDay,

    Guid CorrelationId
) : IRequest<AddCardResult>;

public record AddCardResult(
    Guid CardId,
    string MaskedNumber,
    string IssuerName,
    string Message);



    