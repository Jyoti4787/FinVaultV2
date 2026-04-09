using FinVault.CardService.Domain.Entities;
using FinVault.CardService.Domain.Events;
using FinVault.CardService.Domain.Interfaces;
using MediatR;

namespace FinVault.CardService.Application.Commands.AddCard;

// Published by : MediatR routes here when AddCardCommand sent
// Consumed by  : Nobody — saves card to DB and fires event
public class AddCardCommandHandler
    : IRequestHandler<AddCardCommand, AddCardResult>
{
    private readonly ICreditCardRepository _cards;
    private readonly ICardIssuerRepository _issuers;
    private readonly IPublisher _publisher;

    public AddCardCommandHandler(
        ICreditCardRepository cards,
        ICardIssuerRepository issuers,
        IPublisher publisher)
    {
        _cards    = cards;
        _issuers  = issuers;
        _publisher = publisher;
    }

    public async Task<AddCardResult> Handle(
        AddCardCommand cmd,
        CancellationToken ct)
    {
        // Step 1 — Luhn validation
        // Luhn algorithm checks if card number is valid
        // Every real card number passes this check
        if (!IsValidLuhn(cmd.CardNumber))
            throw new InvalidOperationException(
                "Invalid card number.");

        // Step 2 — Detect issuer from card number
        // Look at the first few digits
        var issuerName = DetectIssuer(cmd.CardNumber);
        var issuer = await _issuers.GetByNameAsync(issuerName, ct)
            ?? throw new InvalidOperationException(
                "Card issuer not supported.");

        // Step 3 — Create masked number
        // "4111111111111111" → "**** **** **** 1111"
        var masked = MaskCardNumber(cmd.CardNumber);

        // Step 4 — Check if this card already added
        if (await _cards.ExistsAsync(cmd.UserId, masked, ct))
            throw new InvalidOperationException(
                "This card is already added.");

        // Step 5 — Create and save card
        var card = new CreditCard
        {
            Id                  = Guid.NewGuid(),
            UserId              = cmd.UserId,
            MaskedNumber        = masked,
            CardholderName      = cmd.CardholderName,
            ExpiryMonth         = cmd.ExpiryMonth,
            ExpiryYear          = cmd.ExpiryYear,
            IssuerId            = issuer.Id,
            CreditLimit         = cmd.CreditLimit,
            OutstandingBalance  = 0,
            BillingCycleStartDay = cmd.BillingCycleStartDay,
            IsDefault           = false,
            IsVerified          = false,
            CreatedAt           = DateTimeOffset.UtcNow
        };

        await _cards.AddAsync(card, ct);

        // Step 6 — Fire domain event
        await _publisher.Publish(
            new CardAddedDomainEvent(
                card.Id,
                card.UserId,
                card.MaskedNumber,
                issuer.Name,
                cmd.CorrelationId), ct);

        return new AddCardResult(
            card.Id,
            card.MaskedNumber,
            issuer.Name,
            "Card added successfully.");
    }

    // LUHN ALGORITHM
    // Every real credit card number passes this check
    // It catches typos in card numbers
    // Example: 4111111111111111 passes
    //          4111111111111112 fails
    private static bool IsValidLuhn(string number)
    {
        // Remove spaces if any
        number = number.Replace(" ", "");

        // Must be all digits
        if (!number.All(char.IsDigit)) return false;

        int sum = 0;
        bool alternate = false;

        for (int i = number.Length - 1; i >= 0; i--)
        {
            int digit = number[i] - '0';

            if (alternate)
            {
                digit *= 2;
                if (digit > 9) digit -= 9;
            }

            sum += digit;
            alternate = !alternate;
        }

        // Valid if sum is divisible by 10
        return sum % 10 == 0;
    }

    // ISSUER DETECTION
    // Based on first few digits of card number
    private static string DetectIssuer(string number)
    {
        // Visa: starts with 4
        if (number.StartsWith("4")) return "Visa";

        // Amex: starts with 34 or 37 (15 digits)
        if (number.StartsWith("34") ||
            number.StartsWith("37")) return "Amex";

        // MasterCard: starts with 51-55 or 2221-2720
        if (number.Length >= 2)
        {
            var prefix2 = int.Parse(number[..2]);
            if (prefix2 >= 51 && prefix2 <= 55)
                return "MasterCard";
        }
        if (number.Length >= 4)
        {
            var prefix4 = int.Parse(number[..4]);
            if (prefix4 >= 2221 && prefix4 <= 2720)
                return "MasterCard";
        }

        // RuPay: starts with 60, 65, 81, 82, 508
        if (number.StartsWith("60") ||
            number.StartsWith("65") ||
            number.StartsWith("81") ||
            number.StartsWith("82") ||
            number.StartsWith("508")) return "RuPay";

        return "Visa"; // default
    }

    // MASK CARD NUMBER
    // "4111111111111111" → "**** **** **** 1111"
    private static string MaskCardNumber(string number)
    {
        number = number.Replace(" ", "");
        var last4 = number[^4..];
        return $"**** **** **** {last4}";
    }
}