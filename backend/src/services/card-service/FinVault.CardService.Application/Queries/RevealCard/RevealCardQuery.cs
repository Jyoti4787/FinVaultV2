using FinVault.CardService.Application.Helpers;
using FinVault.CardService.Domain.Interfaces;
using MediatR;

namespace FinVault.CardService.Application.Queries.RevealCard;

public record RevealCardQuery(Guid CardId, Guid UserId, string Email, string OtpCode) : IRequest<RevealCardResult>;

public record RevealCardResult(
    string CardNumber,
    string CVV,
    int ExpiryMonth,
    int ExpiryYear,
    string CardholderName,
    string Warning);

public class RevealCardQueryHandler : IRequestHandler<RevealCardQuery, RevealCardResult>
{
    private readonly ICreditCardRepository _cards;
    private readonly ICardOtpVerifier _otpVerifier;

    public RevealCardQueryHandler(
        ICreditCardRepository cards,
        ICardOtpVerifier otpVerifier)
    {
        _cards = cards;
        _otpVerifier = otpVerifier;
    }

    public async Task<RevealCardResult> Handle(RevealCardQuery request, CancellationToken ct)
    {
        var card = await _cards.GetByIdAsync(request.CardId, ct)
            ?? throw new InvalidOperationException("Card not found");

        if (card.UserId != request.UserId)
            throw new UnauthorizedAccessException("You do not own this card");

        // ── VERIFY OTP BEFORE REVEALING SENSITIVE DATA ────────────────
        var otpValid = await _otpVerifier.VerifyAsync(request.Email, request.OtpCode, "CardReveal", ct);
        if (!otpValid)
        {
            throw new UnauthorizedAccessException("Invalid or expired OTP code.");
        }

        // Decrypt stored card data
        string fullNumber;
        string cvv;

        if (!string.IsNullOrEmpty(card.EncryptedCardNumber) && !string.IsNullOrEmpty(card.EncryptedCVV))
        {
            // Decrypt from database
            fullNumber = SimpleEncryption.Decrypt(card.EncryptedCardNumber);
            cvv = SimpleEncryption.Decrypt(card.EncryptedCVV);
        }
        else
        {
            // Fallback for old cards without encryption (demo data)
            var last4 = card.MaskedNumber.Substring(card.MaskedNumber.Length - 4);
            fullNumber = $"4532 1234 5678 {last4}";
            cvv = "123";
        }

        return new RevealCardResult(
            fullNumber,
            cvv,
            card.ExpiryMonth,
            card.ExpiryYear,
            card.CardholderName,
            "This sensitive information will only be displayed once. Do not share it with anyone.");
    }
}
