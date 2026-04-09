using FinVault.CardService.Application.Commands.AddCard;
using FluentValidation;

namespace FinVault.CardService.Application.Validators;

public class AddCardValidator : AbstractValidator<AddCardCommand>
{
    public AddCardValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Card number is required.")
            .Length(15, 16)
            .WithMessage("Card number must be 15 or 16 digits.");

        RuleFor(x => x.Cvv)
            .NotEmpty().WithMessage("CVV is required.")
            .Matches(@"^\d{3,4}$").WithMessage("CVV must be 3 or 4 digits.");

        RuleFor(x => x.CardholderName)
            .NotEmpty()
            .WithMessage("Cardholder name is required.");

        RuleFor(x => x.ExpiryMonth)
            .InclusiveBetween(1, 12)
            .WithMessage("Expiry month must be between 1 and 12.");

        RuleFor(x => x.ExpiryYear)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Year)
            .WithMessage("Card has expired.");

        RuleFor(x => x.CreditLimit)
            .GreaterThan(0)
            .WithMessage("Credit limit must be greater than zero.");

        RuleFor(x => x.BillingCycleStartDay)
            .InclusiveBetween(1, 28)
            .WithMessage("Billing cycle start day must be between 1 and 28.");
    }
}