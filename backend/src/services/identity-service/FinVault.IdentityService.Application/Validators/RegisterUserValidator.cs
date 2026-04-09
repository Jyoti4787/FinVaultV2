using FinVault.IdentityService.Application.Commands.RegisterUser;
using FluentValidation;

namespace FinVault.IdentityService.Application.Validators;

// WHAT IS A VALIDATOR?
// It is a GATEKEEPER
// Before the command handler even runs
// The validator checks — is the data correct?
//
// Think of it like a form on a website
// If you leave email blank → "Email is required"
// If password is too short → "Min 8 characters"
// These checks happen BEFORE any database call
//
// WHY use FluentValidation?
// It is a popular library for clean validation rules
// Much better than if/else checks in handler
// Rules are readable like plain English

// Runs automatically before RegisterUserCommandHandler
// If validation fails → handler never runs
// Returns 400 Bad Request with error messages
public class RegisterUserValidator
    : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        // Rule 1 — Email must not be empty
        // AND must be a valid email format
        // "jyoti" fails — not a valid email
        // "jyoti@gmail.com" passes
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.");

        // Rule 2 — Password must not be empty
        // AND must be at least 8 characters
        // "abc" fails — too short
        // "MyPass123" passes
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage(
                "Password must be at least 8 characters.");

        // Rule 3 — First name must not be empty
        // "Jyoti" passes
        // "" or null fails
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required.");

        // Rule 4 — Last name must not be empty
        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required.");
    }
}